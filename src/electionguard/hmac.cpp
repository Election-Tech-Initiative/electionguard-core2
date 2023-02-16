#include "electionguard/hmac.hpp"

#include "../../libs/hacl/Hacl_HMAC.hpp"
#include "../../libs/hacl/Lib.hpp"
#include "log.hpp"

#include <iomanip>
#include <iostream>

using hacl::HMACAlgorithm;
using hacl::Lib;
using std::get;
using std::make_unique;
using std::nullptr_t;
using std::out_of_range;
using std::reference_wrapper;
using std::string;
using std::to_string;
using std::unique_ptr;
using std::vector;

namespace electionguard
{
    vector<uint8_t> HMAC::compute(const vector<uint8_t> &key, const vector<uint8_t> &message,
                                  uint32_t length, uint32_t start)
    {
        vector<uint8_t> data_to_hmac;

        if (length > 0) {
            data_to_hmac.insert(data_to_hmac.end(), (uint8_t *)&start,
                                (uint8_t *)&start + sizeof(start));
            data_to_hmac.insert(data_to_hmac.end(), message.begin(), message.end());
            data_to_hmac.insert(data_to_hmac.end(), (uint8_t *)&length,
                                (uint8_t *)&length + sizeof(length));
        } else {
            data_to_hmac = message;
        }

        // calculate the hmac and then zeroize the buffer holding the data we hmaced
        auto hmac = hacl::HMAC::compute(key, data_to_hmac, HMACAlgorithm::SHA2_256);
        Lib::memZero(&data_to_hmac.front(), data_to_hmac.size());

        return hmac;
    }

} // namespace electionguard
