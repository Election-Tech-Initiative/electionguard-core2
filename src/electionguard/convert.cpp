#include "convert.hpp"

#include "../../libs/hacl/Hacl_Bignum256.hpp"
#include "date/date.h"
#include "facades/bignum4096.hpp"
#include "krml/lowstar_endianness.h"
#include "log.hpp"

#include <chrono>
#include <codecvt>
#include <iomanip>
#include <iostream>
#include <sstream>
#include <string>
#include <vector>

using electionguard::facades::Bignum4096;
using hacl::Bignum256;
using std::string;
using time_point = std::chrono::system_clock::time_point;
using std::get_time;
using std::gmtime;
using std::mktime;
using std::stringstream;
using std::chrono::system_clock;

namespace electionguard
{
    vector<uint8_t> bignum_to_bytes(const vector<uint64_t> &bignum)
    {
        size_t offset = sizeof(uint64_t) / sizeof(uint8_t);
        std::vector<uint8_t> bytes;
        bytes.reserve((bignum.size() * offset));
        for (auto number : bignum) {
            uint64_t buffer = htobe64(number);
            for (size_t i = 0; i < sizeof(buffer); i++) {
                bytes.push_back(buffer & 0xFF);
                buffer >>= 8;
            }
        }
        return bytes;
    }

    vector<uint8_t> bignum256_to_bytes(uint64_t *bytes)
    {
        uint8_t byteResult[MAX_Q_SIZE] = {};
        Bignum256::toBytes(bytes, byteResult);
        vector<uint8_t> result(std::begin(byteResult), std::end(byteResult));
        return result;
    }

    string hacl_to_hex_256(uint64_t *data)
    {
        // Returned bytes array from Hacl needs to be pre-allocated to 32 bytes
        uint8_t byteResult[MAX_Q_SIZE] = {};
        // Use Hacl to convert the bignum to byte array
        hacl::Bignum256::toBytes(static_cast<uint64_t *>(data), static_cast<uint8_t *>(byteResult));
        return bytes_to_hex(byteResult);
    }

    string hacl_to_hex_4096(uint64_t *data)
    {
        // Returned bytes array from Hacl needs to be pre-allocated to 512 bytes
        uint8_t byteResult[MAX_P_SIZE] = {};
        // Use Hacl to convert the bignum to byte array
        Bignum4096::toBytes(static_cast<uint64_t *>(data), static_cast<uint8_t *>(byteResult));
        return bytes_to_hex(byteResult);
    }

    const string defaultFormat = "%FT%TZ";
    const string secondaryFormat = "%FT%T%Ez";

    string timePointToIsoString(const time_point &time, const string &format)
    {
        auto c_time = system_clock::to_time_t(time);
        struct tm gmt;

#ifdef _WIN32
        // TODO: ISSUE #136: handle err
        gmtime_s(&gmt, &c_time);
#else
        gmtime_r(&c_time, &gmt);
#endif
        std::ostringstream ss;
        ss << std::put_time(&gmt, format.c_str());
        return ss.str();
    }

    string timePointToIsoString(const time_point &time)
    {
        return timePointToIsoString(time, defaultFormat);
    }

    time_point timePointFromIsoString(const string &time, const string &format)
    {
        date::sys_time<std::chrono::seconds> tm;
        std::istringstream ss{time};
        ss >> date::parse(format, tm);
        if (ss.fail()) {
            ss.clear();
            ss.str(time);
            ss >> date::parse(secondaryFormat, tm);
        }

        auto testTime = timePointToIsoString(tm, format);
        return tm;
    }

    time_point timePointFromIsoString(const string &time)
    {
        return timePointFromIsoString(time, defaultFormat);
    }

} // namespace electionguard
