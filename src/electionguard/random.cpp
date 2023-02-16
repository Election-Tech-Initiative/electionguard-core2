#include "random.hpp"

#include "../../libs/hacl/Hacl_HMAC_DRBG.hpp"
#include "../../libs/hacl/Lib.hpp"
#include "convert.hpp"
#include "log.hpp"

#include <ctime>
#include <iomanip>
#include <iostream>
#include <sstream>
#include <string>
#include <vector>

using hacl::HMAC_DRBG;
using hacl::HMAC_DRBG_HashAlgorithm;
using hacl::Lib;
using std::bad_alloc;
using std::put_time;
using std::stringstream;
using std::vector;

namespace electionguard
{
    static void cleanup(vector<uint8_t> entropy, vector<uint8_t> nonce)
    {
        release(entropy);
        release(nonce);
    }

    /// <summary>
    /// Get random bytes from the operating system using the default generator
    ///
    /// Depending on the OS, this may be a blocking or non-blocking call
    /// see: https://github.com/project-everest/hacl-star/blob/master/dist/election-guard/Lib_RandomBuffer_System.c
    static vector<uint8_t> getRandomBytes(uint32_t count = 256)
    {
        // TODO: ISSUE #137: use unique_ptr or vector instead of a direct heap allocation
        auto *array = new uint8_t[count];
        if (Lib::readRandomBytes(count, array, false)) {
            vector<uint8_t> result(array, array + count);
            delete[] array;
            return result;

        } else {
            delete[] array;
            throw bad_alloc();
        }
    }
    string getTime()
    {
        auto now_seconds = time(nullptr);
        struct tm now;

        stringstream stream;
#ifdef _WIN32
        // TODO: ISSUE #136: handle error
        gmtime_s(&now, &now_seconds);
        stream << put_time(&now, "%c");
#else
        stream << put_time(gmtime_r(&now_seconds, &now), "%c");
#endif
        return stream.str();
    }

    vector<uint8_t> Random::getBytes(ByteSize size)
    {
        // Get some random bytes from the operating system
        auto entropy = getRandomBytes(static_cast<uint32_t>(size * 2));

        // Allocate the DRBG
        auto state = HMAC_DRBG{HMAC_DRBG_HashAlgorithm::SHA2_256};

        // Derive a nonce from the OS entropy pool
        auto nonce = getRandomBytes(size);

        // Derive a personalization string from the system clock
        auto personalization = getTime();

        // Instantiate the DRBG
        state.instantiate(convert(entropy.size()), entropy.data(), convert(nonce.size()),
                          nonce.data(), convert(personalization.size()),
                          reinterpret_cast<uint8_t *>(personalization.data()));

        auto *array = new uint8_t[size];
        auto input = getRandomBytes(size);

        // Try to generate some random bits
        if (state.generate(array, size, convert(input.size()), input.data())) {
            vector<uint8_t> result(array, array + size);
            cleanup(entropy, nonce);
            delete[] array;
            return result;
        } else {
            cleanup(entropy, nonce);
            delete[] array;
            throw bad_alloc();
        }
    }
} // namespace electionguard
