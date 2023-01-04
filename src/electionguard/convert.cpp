#include "date/date.h"
#include "krml/lowstar_endianness.h"
#include "log.hpp"

#include <chrono>
#include <codecvt>
#include <iomanip>
#include <iostream>
#include <sstream>
#include <string>
#include <vector>

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
