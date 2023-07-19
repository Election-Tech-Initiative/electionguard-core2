#ifndef __ELECTIONGUARD_CPP_HASH_HPP_INCLUDED__
#define __ELECTIONGUARD_CPP_HASH_HPP_INCLUDED__
#include <electionguard/crypto_hashable.hpp>
#include <electionguard/export.h>
#include <electionguard/group.hpp>
#include <memory>
#include <string>
#include <variant>
#include <vector>

namespace electionguard
{
    using CryptoHashableType = std::variant<
      std::nullptr_t, CryptoHashable *, ElementModP *, ElementModQ *,
      std::reference_wrapper<CryptoHashable>, std::reference_wrapper<ElementModP>,
      std::reference_wrapper<ElementModQ>, std::reference_wrapper<const CryptoHashable>,
      std::reference_wrapper<const ElementModP>, std::reference_wrapper<const ElementModQ>,
      uint64_t, std::string, std::vector<CryptoHashable *>, std::vector<ElementModP *>,
      std::vector<ElementModQ *>, std::vector<std::reference_wrapper<CryptoHashable>>,
      std::vector<std::reference_wrapper<ElementModP>>,
      std::vector<std::reference_wrapper<ElementModQ>>,
      std::vector<std::reference_wrapper<const CryptoHashable>>,
      std::vector<std::reference_wrapper<const ElementModP>>,
      std::vector<std::reference_wrapper<const ElementModQ>>, std::vector<uint64_t>,
      std::vector<std::string>, std::vector<uint8_t>>;

#ifdef __cplusplus
    extern "C" {
#endif

    const char EG_ELECTIONGUARD_HASH_PREFIX_00[] = "00"; // Manifest Hash
    const char EG_ELECTIONGUARD_HASH_PREFIX_01[] = "01"; // Guardian Key Proof
    const char EG_ELECTIONGUARD_HASH_PREFIX_02[] = "02"; // Guardian KeyShare Encryption Proof
    const char EG_ELECTIONGUARD_HASH_PREFIX_03[] = "03"; // Election Extended Base Hash
    const char EG_ELECTIONGUARD_HASH_PREFIX_04[] = "04"; // Ballot Selection Encryption Proof
    const char EG_ELECTIONGUARD_HASH_PREFIX_05[] = "05"; // Ballot Contest Data Secret Key
    const char EG_ELECTIONGUARD_HASH_PREFIX_06[] = "06"; // Ballot Selection Decryption Proof

#ifdef __cplusplus
    }
#endif

    /// <Summary>
    /// Hash constant used to prefix the hash of election artifacts
    /// </Summary>
    class EG_API HashPrefix
    {
      public:
        /// <Summary>
        /// Get the prefix for the hash of an election manifest
        /// </Summary>
        static const std::string &get_prefix_00()
        {
            static const std::string prefix_00(EG_ELECTIONGUARD_HASH_PREFIX_00);
            return prefix_00;
        }

        /// <Summary>
        /// Get the prefix for the hash of a guardian key proof
        /// </Summary>
        static const std::string &get_prefix_01()
        {
            static const std::string prefix_01(EG_ELECTIONGUARD_HASH_PREFIX_01);
            return prefix_01;
        }

        /// <Summary>
        /// Get the prefix for the hash of a guardian key share encryption proof
        /// </Summary>
        static const std::string &get_prefix_02()
        {
            static const std::string prefix_02(EG_ELECTIONGUARD_HASH_PREFIX_02);
            return prefix_02;
        }

        /// <Summary>
        /// Get the prefix for the hash of an election extended base hash
        /// </Summary>
        static const std::string &get_prefix_03()
        {
            static const std::string prefix_03(EG_ELECTIONGUARD_HASH_PREFIX_03);
            return prefix_03;
        }

        /// <Summary>
        /// Get the prefix for the hash of a ballot selection encryption proof
        /// </Summary>
        static const std::string &get_prefix_04()
        {
            static const std::string prefix_04(EG_ELECTIONGUARD_HASH_PREFIX_04);
            return prefix_04;
        }

        /// <Summary>
        /// Get the prefix for the hash of a ballot contest data secret key
        /// </Summary>
        static const std::string &get_prefix_05()
        {
            static const std::string prefix_05(EG_ELECTIONGUARD_HASH_PREFIX_05);
            return prefix_05;
        }

        /// <Summary>
        /// Get the prefix for the hash of a ballot selection decryption proof
        /// </Summary>
        static const std::string &get_prefix_06()
        {
            static const std::string prefix_06(EG_ELECTIONGUARD_HASH_PREFIX_06);
            return prefix_06;
        }
    };

    /// <Summary>
    /// Given zero or more elements, calculate their cryptographic hash
    /// using SHA256. Allowed element types are `ElementModP`, `ElementModQ`,
    /// `str`, or `int`, anything implementing `CryptoHashable`, and vectors
    /// or references of any of those types.

    /// <param name="a"> Zero or more elements of any of the accepted types.</param>
    /// <returns>A cryptographic hash of these elements, concatenated.</returns>
    /// </Summary>
    EG_API std::unique_ptr<ElementModQ> hash_elems(const std::vector<CryptoHashableType> &a);

    /// <Summary>
    /// Given zero or more elements, calculate their cryptographic hash
    /// using SHA256. Allowed element types are `ElementModP`, `ElementModQ`,
    /// `str`, or `int`, anything implementing `CryptoHashable`, and vectors
    /// or references of any of those types.

    /// <param name="a"> Zero or more elements of any of the accepted types.</param>
    /// <returns>A cryptographic hash of these elements, concatenated.</returns>
    /// </Summary>
    EG_API std::unique_ptr<ElementModQ> hash_elems(CryptoHashableType a);
} // namespace electionguard

#endif /* __ELECTIONGUARD_CPP_HASH_HPP_INCLUDED__ */
