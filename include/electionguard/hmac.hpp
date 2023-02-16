#ifndef __ELECTIONGUARD_CPP_HMAC_HPP_INCLUDED__
#define __ELECTIONGUARD_CPP_HMAC_HPP_INCLUDED__
#include <electionguard/crypto_hashable.hpp>
#include <electionguard/export.h>
#include <electionguard/group.hpp>
#include <memory>
#include <string>
#include <variant>
#include <vector>

namespace electionguard
{
    class HMAC
    {
      public:
        /// <param name="a"> Zero or more elements of any of the accepted types.</param>
        /// <returns>A cryptographic hash of these elements, concatenated.</returns>
        /// </Summary>
        EG_API static std::vector<uint8_t> compute(const std::vector<uint8_t> &key,
                                                   const std::vector<uint8_t> &message,
                                                   uint32_t length, uint32_t start);
    };

} // namespace electionguard

#endif /* __ELECTIONGUARD_CPP_HMAC_HPP_INCLUDED__ */
