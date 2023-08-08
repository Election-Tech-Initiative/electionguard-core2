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

    // HP = H(HV ;00,p,q,g). Parameter Hash 3.1.2
    const char EG_ELECTIONGUARD_HASH_PREFIX_PARAMETER_HASH[] = "00";

    // HM = H(HP;01,manifest). Manifest Hash 3.1.4
    const char EG_ELECTIONGUARD_HASH_PREFIX_MANIFEST_HASH[] = "01";

    // HB = (HP;02,n,k,date,info,HM). Election Base Hash 3.1.5
    const char EG_ELECTIONGUARD_HASH_PREFIX_ELECTION_BASE_HASH[] = "02";

    // H = (HP ; 10, i, j, Ki,j , hi,j ). Guardin Share proof challenge 3.2.2
    const char EG_ELECTIONGUARD_HASH_PREFIX_GUARDIAN_SHARE_CHALLENGE[] = "10";

    // ki,l = H(HP;11,i,l,Kl,αi,l,βi,l). (14) Guardain Share Encryption Secret Key 3.2.2
    const char EG_ELECTIONGUARD_HASH_PREFIX_GUARDIAN_SHARE_SECRET[] = "11";

    // HE = H(HB;12,K,K1,0,K1,1,...,K1,k−1,K2,0,...,Kn,k−2,Kn,k−1). Extended Base Hash 3.2.3
    const char EG_ELECTIONGUARD_HASH_PREFIX_ELECTION_EXTENDED_HASH[] = "12";

    // ξi,j = H(HE;20,ξB,Λi,λj). encryption nonce 3.3.2
    const char EG_ELECTIONGUARD_HASH_PREFIX_SELECTION_NONCE[] = "20";

    // c = H(HE;21,K,α,β,a0,b0,a1,b1,...,aL,bL). Ballot Selection Encryption Proof (ranged) 3.3.5
    // c = H(HE;21,K,α,β,a0,b0,a1,b1). Ballot Selection Encryption Proof (unselected) 3.3.5
    // c = H(HE;21,K,α,β,a0,b0,a1,b1). Ballot Selection Encryption Proof (selected) 3.3.5
    const char EG_ELECTIONGUARD_HASH_PREFIX_SELECTION_PROOF[] = "21";

    // ξ = H (HE ; 20, ξB , Λ, ”contest data”). Ballot Contest Data Nonce 3.3.6
    const char EG_ELECTIONGUARD_HASH_PREFIX_CONTEST_DATA_NONCE[] = "20";

    // k = H(HE;22,K,α,β). Ballot ContestData Secret Key 3.3.6
    const char EG_ELECTIONGUARD_HASH_PREFIX_CONTEST_DATA_SECRET[] = "22";

    // c = H(HE;21,K,α ̄,β ̄,a0,b0,a1,b1,...,aL,bL). Ballot Contest Limit Encryption Proof 3.3.8
    const char EG_ELECTIONGUARD_HASH_PREFIX_CONTEST_PROOF[] = "21";

    // χl = H(HE;23,Λl,K,α1,β1,α2,β2 ...,αm,βm). Contest Hash 3.4.1
    const char EG_ELECTIONGUARD_HASH_PREFIX_CONTEST_HASH[] = "23";

    // H(B) = H(HE;24,χ1,χ2,...,χmB ,Baux). Confirmation Code 3.4.2
    const char EG_ELECTIONGUARD_HASH_PREFIX_BALLOT_CODE[] = "24";

    // H0 = H(HE;24,Baux,0), Ballot Chaining for Fixed Device 3.4.3
    // H = H(HE;24,Baux), Ballot chaining closure
    const char EG_ELECTIONGUARD_HASH_PREFIX_BALLOT_CHAIN[] = "24";

    // c = H(HE;30,K,A,B,a,b,M). Ballot Selection Decryption Proof 3.6.3
    // c = H(HE;30,K,α,β,a,b,M). Challenge Ballot Selection Decryption Proof 3.6.5
    const char EG_ELECTIONGUARD_HASH_PREFIX_DECRYPT_SELECTION_PROOF[] = "30";

    // c = H(HE;31,K,C0,C1,C2,a,b,β). Ballot Contest Decryption of Contest Data 3.6.4
    const char EG_ELECTIONGUARD_HASH_PREFIX_DECRYPT_CONTEST_DATA[] = "31";
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
        /// HP = H(HV ;00,p,q,g). Parameter Hash 3.1.2
        /// </Summary>
        static const std::string &get_prefix_parameter_hash()
        {
            static const std::string prefix(EG_ELECTIONGUARD_HASH_PREFIX_PARAMETER_HASH);
            return prefix;
        }

        /// <Summary>
        /// HM = H(HP;01,manifest). Manifest Hash 3.1.4
        /// </Summary>
        static const std::string &get_prefix_manifest_hash()
        {
            static const std::string prefix(EG_ELECTIONGUARD_HASH_PREFIX_MANIFEST_HASH);
            return prefix;
        }

        /// <Summary>
        /// HB = (HP;02,n,k,date,info,HM). Election Base Hash 3.1.5
        /// </Summary>
        static const std::string &get_prefix_base_hash()
        {
            static const std::string prefix(EG_ELECTIONGUARD_HASH_PREFIX_ELECTION_BASE_HASH);
            return prefix;
        }

        /// <Summary>
        /// H = (HP ; 10, i, j, Ki,j , hi,j ). Guardin Share proof challenge 3.2.2
        /// </Summary>
        static const std::string &get_prefix_guardian_share_challenge()
        {
            static const std::string prefix(EG_ELECTIONGUARD_HASH_PREFIX_GUARDIAN_SHARE_CHALLENGE);
            return prefix;
        }

        /// <Summary>
        /// ki,l = H(HP;11,i,l,Kl,αi,l,βi,l). (14) Guardain Share Encryption Secret Key 3.2.2
        /// </Summary>
        static const std::string &get_prefix_guardian_share_secret()
        {
            static const std::string prefix(EG_ELECTIONGUARD_HASH_PREFIX_GUARDIAN_SHARE_SECRET);
            return prefix;
        }

        /// <Summary>
        /// HE = H(HB;12,K,K1,0,K1,1,...,K1,k−1,K2,0,...,Kn,k−2,Kn,k−1). Extended Base Hash 3.2.3
        /// </Summary>
        static const std::string &get_prefix_extended_hash()
        {
            static const std::string prefix(EG_ELECTIONGUARD_HASH_PREFIX_ELECTION_EXTENDED_HASH);
            return prefix;
        }

        /// <Summary>
        /// ξi,j = H(HE;20,ξB,Λi,λj). encryption nonce 3.3.2
        /// </Summary>
        static const std::string &get_prefix_selection_nonce()
        {
            static const std::string prefix(EG_ELECTIONGUARD_HASH_PREFIX_SELECTION_NONCE);
            return prefix;
        }

        /// <Summary>
        /// c = H(HE;21,K,α,β,a0,b0,a1,b1,...,aL,bL). Ballot Selection Encryption Proof (ranged) 3.3.5
        /// c = H(HE;21,K,α,β,a0,b0,a1,b1). Ballot Selection Encryption Proof (unselected) 3.3.5
        /// c = H(HE;21,K,α,β,a0,b0,a1,b1). Ballot Selection Encryption Proof (selected) 3.3.5
        /// </Summary>
        static const std::string &get_prefix_selection_proof()
        {
            static const std::string prefix(EG_ELECTIONGUARD_HASH_PREFIX_SELECTION_PROOF);
            return prefix;
        }

        /// <Summary>
        /// ξ = H (HE ; 20, ξB , Λ, ”contest data”). Ballot Contest Data Nonce 3.3.6
        /// </Summary>
        static const std::string &get_prefix_contest_data_nonce()
        {
            static const std::string prefix(EG_ELECTIONGUARD_HASH_PREFIX_CONTEST_DATA_NONCE);
            return prefix;
        }

        /// <Summary>
        /// k = H(HE;22,K,α,β). Ballot ContestData Secret Key 3.3.6
        /// </Summary>
        static const std::string &get_prefix_contest_data_secret()
        {
            static const std::string prefix(EG_ELECTIONGUARD_HASH_PREFIX_CONTEST_DATA_SECRET);
            return prefix;
        }

        /// <Summary>
        /// c = H(HE;21,K,α ̄,β ̄,a0,b0,a1,b1,...,aL,bL). Ballot Contest Limit Encryption Proof 3.3.8
        /// </Summary>
        static const std::string &get_prefix_contest_proof()
        {
            static const std::string prefix(EG_ELECTIONGUARD_HASH_PREFIX_CONTEST_PROOF);
            return prefix;
        }

        /// <Summary>
        /// χl = H(HE;23,Λl,K,α1,β1,α2,β2 ...,αm,βm). Contest Hash 3.4.1
        /// </Summary>
        static const std::string &get_prefix_contest_hash()
        {
            static const std::string prefix(EG_ELECTIONGUARD_HASH_PREFIX_CONTEST_HASH);
            return prefix;
        }

        /// <Summary>
        /// H(B) = H(HE;24,χ1,χ2,...,χmB ,Baux). Confirmation Code 3.4.2
        /// </Summary>
        static const std::string &get_prefix_ballot_code()
        {
            static const std::string prefix(EG_ELECTIONGUARD_HASH_PREFIX_BALLOT_CODE);
            return prefix;
        }

        /// <Summary>
        /// H0 = H(HE;24,Baux,0), Ballot Chaining for Fixed Device 3.4.3
        /// H = H(HE;24,Baux), Ballot chaining closure
        /// </Summary>
        static const std::string &get_prefix_ballot_chain()
        {
            static const std::string prefix(EG_ELECTIONGUARD_HASH_PREFIX_BALLOT_CHAIN);
            return prefix;
        }

        /// <Summary>
        /// c = H(HE;30,K,A,B,a,b,M). Ballot Selection Decryption Proof 3.6.3
        /// c = H(HE;30,K,α,β,a,b,M). Challenge Ballot Selection Decryption Proof 3.6.5
        /// </Summary>
        static const std::string &get_prefix_decrypt_selection_proof()
        {
            static const std::string prefix(EG_ELECTIONGUARD_HASH_PREFIX_DECRYPT_SELECTION_PROOF);
            return prefix;
        }

        /// <Summary>
        /// c = H(HE;31,K,C0,C1,C2,a,b,β). Ballot Contest Decryption of Contest Data 3.6.4
        /// </Summary>
        static const std::string &get_prefix_decrypt_contest_data()
        {
            static const std::string prefix(EG_ELECTIONGUARD_HASH_PREFIX_DECRYPT_CONTEST_DATA);
            return prefix;
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
