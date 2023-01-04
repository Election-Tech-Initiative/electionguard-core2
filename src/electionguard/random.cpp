#include "random.hpp"

#include "Hacl_HMAC_DRBG.h"
#include "Lib_RandomBuffer_System.h"
#include "convert.hpp"
#include "log.hpp"

#include <ctime>
#include <iomanip>
#include <iostream>
#include <sstream>
#include <string>
#include <vector>

using std::bad_alloc;
using std::put_time;
using std::stringstream;
using std::vector;

namespace electionguard
{
    // statically pin the hashing algorithm to SHA256
    static const Spec_Hash_Definitions_hash_alg hashAlgo =
      static_cast<Spec_Hash_Definitions_hash_alg>(Spec_Hash_Definitions_SHA2_256);

    static void cleanup(Hacl_HMAC_DRBG_state &state, vector<uint8_t> entropy, vector<uint8_t> nonce)
    {
        release(entropy);
        release(nonce);
        free(state.k);
        free(state.v);
        free(state.reseed_counter);
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
        if (Lib_RandomBuffer_System_randombytes(array, count)) {
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
        Hacl_HMAC_DRBG_state state = Hacl_HMAC_DRBG_create_in(hashAlgo);

        // Derive a nonce from the OS entropy pool
        auto nonce = getRandomBytes(size);

        // Derive a personalization string from the system clock
        auto personalization = getTime();

        // Instantiate the DRBG
        Hacl_HMAC_DRBG_instantiate(hashAlgo, state, convert(entropy.size()), entropy.data(),
                                   convert(nonce.size()), nonce.data(),
                                   convert(personalization.size()),
                                   reinterpret_cast<uint8_t *>(personalization.data()));

        auto *array = new uint8_t[size];
        auto input = getRandomBytes(size);

        // Try to generate some random bits
        if (Hacl_HMAC_DRBG_generate(hashAlgo, array, state, size, convert(input.size()),
                                    input.data())) {
            vector<uint8_t> result(array, array + size);
            cleanup(state, entropy, nonce);
            delete[] array;
            return result;
        } else {
            cleanup(state, entropy, nonce);
            delete[] array;
            throw bad_alloc();
        }
    }
} // namespace electionguard
