#ifndef __ELECTIONGUARD_CPP_CHAUM_PEDERSEN_HPP_INCLUDED__
#define __ELECTIONGUARD_CPP_CHAUM_PEDERSEN_HPP_INCLUDED__

#include "elgamal.hpp"
#include "export.h"
#include "group.hpp"
#include "precompute_buffers.hpp"

#include <memory>

namespace electionguard
{

    /// <Summary>
    /// The Disjunctive Chaum Pederson proof is a Non-Interactive Zero-Knowledge Proof
    /// that represents the proof of ballot correctness (that a value is either zero or one).
    /// This proof demonstrates that an ElGamal encryption pair (𝛼,𝛽) is an encryption of zero or one
    /// (given knowledge of encryption nonce R).
    ///
    /// This object should not be constructed directly.  Use DisjunctiveChaumPedersenProof::make
    ///
    /// see: https://www.electionguard.vote/spec/0.95.0/5_Ballot_encryption/#outline-for-proofs-of-ballot-correctness
    /// </Summary>
    class EG_API DisjunctiveChaumPedersenProof
    {
      public:
        DisjunctiveChaumPedersenProof(const DisjunctiveChaumPedersenProof &other);
        DisjunctiveChaumPedersenProof(const DisjunctiveChaumPedersenProof &&other);
        DisjunctiveChaumPedersenProof(std::unique_ptr<ElementModP> proof_zero_pad,
                                      std::unique_ptr<ElementModP> proof_zero_data,
                                      std::unique_ptr<ElementModP> proof_one_pad,
                                      std::unique_ptr<ElementModP> proof_one_data,
                                      std::unique_ptr<ElementModQ> proof_zero_challenge,
                                      std::unique_ptr<ElementModQ> proof_one_challenge,
                                      std::unique_ptr<ElementModQ> challenge,
                                      std::unique_ptr<ElementModQ> proof_zero_response,
                                      std::unique_ptr<ElementModQ> proof_one_response);
        ~DisjunctiveChaumPedersenProof();

        DisjunctiveChaumPedersenProof &operator=(DisjunctiveChaumPedersenProof other);
        DisjunctiveChaumPedersenProof &operator=(DisjunctiveChaumPedersenProof &&other);

        /// <Summary>
        /// a0 in the spec
        /// </Summary>
        ElementModP *getProofZeroPad() const;

        /// <Summary>
        /// b0 in the spec
        /// </Summary>
        ElementModP *getProofZeroData() const;

        /// <Summary>
        /// a1 in the spec
        /// </Summary>
        ElementModP *getProofOnePad() const;

        /// <Summary>
        /// b1 in the spec
        /// </Summary>
        ElementModP *getProofOneData() const;

        /// <Summary>
        /// c0 in the spec
        /// </Summary>
        ElementModQ *getProofZeroChallenge() const;

        /// <Summary>
        /// c1 in the spec
        /// </Summary>
        ElementModQ *getProofOneChallenge() const;

        /// <Summary>
        /// c in the spec
        /// </Summary>
        ElementModQ *getChallenge() const;

        /// <Summary>
        /// v0 in the spec
        /// </Summary>
        ElementModQ *getProofZeroResponse() const;

        /// <Summary>
        /// v1 in the spec
        /// </Summary>
        ElementModQ *getProofOneResponse() const;

        /// <Summary>
        /// make function for a `DisjunctiveChaumPedersenProof`
        ///
        /// This overload does not accept a seed value and calculates
        /// proofs independent of the original encryption. (faster performance)
        /// <param name="message"> The ciphertext message</param>
        /// <param name="r"> The nonce used creating the ElGamal ciphertext</param>
        /// <param name="k"> The public key of the election</param>
        /// <param name="q"> A value used when generating the challenge,
        ///          usually the election extended base hash (𝑄')</param>
        /// <returns>A unique pointer</returns>
        /// </Summary>
        static std::unique_ptr<DisjunctiveChaumPedersenProof>
        make(const ElGamalCiphertext &message, const ElementModQ &r, const ElementModP &k,
             const ElementModQ &q, uint64_t plaintext);

        /// <Summary>
        /// make function for a `DisjunctiveChaumPedersenProof`
        ///
        /// This overload accepts a seed value and calculates
        /// proofs deterministically based on the seed. (slower, but reproduceable proofs)
        /// <param name="message"> The ciphertext message</param>
        /// <param name="r"> The nonce used creating the ElGamal ciphertext</param>
        /// <param name="k"> The public key of the election</param>
        /// <param name="q"> A value used when generating the challenge,
        ///          usually the election extended base hash (𝑄')</param>
        /// <param name="seed">Used to generate other random values here</param>
        /// <returns>A unique pointer</returns>
        /// </Summary>
        static std::unique_ptr<DisjunctiveChaumPedersenProof>
        make(const ElGamalCiphertext &message, const ElementModQ &r, const ElementModP &k,
             const ElementModQ &q, const ElementModQ &seed, uint64_t plaintext);

        /// <Summary>
        /// make function for a `DisjunctiveChaumPedersenProof`
        ///
        /// This overload uses precomputed intermediate values to improve performance.
        /// Because the precomputed values must be known ahead of time,
        /// it does not accept a seed value and calculates
        /// proofs independent of the original encryption. (faster performance)
        /// <param name="message"> The ciphertext message</param>
        /// <param name="precomputedTwoTriplesAndAQuad">A set of intermediate values that were generated ahead of time.</param>
        /// <param name="k"> The public key of the election</param>
        /// <param name="q"> A value used when generating the challenge,
        ///          usually the election extended base hash (𝑄')</param>
        /// <returns>A unique pointer</returns>
        /// </Summary>
        static std::unique_ptr<DisjunctiveChaumPedersenProof>
        make(const ElGamalCiphertext &message,
             const TwoTriplesAndAQuadruple &precomputedTwoTriplesAndAQuad, const ElementModP &k,
             const ElementModQ &q, uint64_t plaintext);

        /// <Summary>
        /// Validates a "disjunctive" Chaum-Pedersen (zero or one) proof.
        ///
        /// <param name="message"> The ciphertext message</param>
        /// <param name="k"> The public key of the election</param>
        /// <param name="q"> The extended base hash of the election</param>
        /// <returns> True if everything is consistent. False otherwise. </returns>
        /// </Summary>
        bool isValid(const ElGamalCiphertext &message, const ElementModP &k, const ElementModQ &q);

        std::unique_ptr<DisjunctiveChaumPedersenProof> clone() const;

      protected:
        static std::unique_ptr<DisjunctiveChaumPedersenProof>
        make_zero(const ElGamalCiphertext &message, const ElementModQ &r, const ElementModP &k,
                  const ElementModQ &q);
        static std::unique_ptr<DisjunctiveChaumPedersenProof>
        make_zero(const ElGamalCiphertext &message, const ElementModQ &r, const ElementModP &k,
                  const ElementModQ &q, const ElementModQ &seed);
        static std::unique_ptr<DisjunctiveChaumPedersenProof>
        make_zero(const ElGamalCiphertext &message,
                  const TwoTriplesAndAQuadruple &precomputedTwoTriplesAndAQuad,
                  const ElementModP &k, const ElementModQ &q);

        static std::unique_ptr<DisjunctiveChaumPedersenProof>
        make_one(const ElGamalCiphertext &message, const ElementModQ &r, const ElementModP &k,
                 const ElementModQ &q);
        static std::unique_ptr<DisjunctiveChaumPedersenProof>
        make_one(const ElGamalCiphertext &message, const ElementModQ &r, const ElementModP &k,
                 const ElementModQ &q, const ElementModQ &seed);
        static std::unique_ptr<DisjunctiveChaumPedersenProof>
        make_one(const ElGamalCiphertext &message,
                 const TwoTriplesAndAQuadruple &precomputedTwoTriplesAndAQuad, const ElementModP &k,
                 const ElementModQ &q);

      private:
        class Impl;
#pragma warning(suppress : 4251)
        std::unique_ptr<Impl> pimpl;
    };

    /// <Summary>
    /// The Constant Chaum PedersenProof is a Non-Interactive Zero-Knowledge Proof
    /// that represents the proof of satisfying the selection limit (that the voter has not over voted).
    /// The proof demonstrates that the elgamal accumulation of the encrypted selections
    /// on the ballot forms an aggregate contest encryption matches the combination of random nonces (R)
    /// used to encrypt the selections and that the encrypted values do not exceed the selection limit L.
    ///
    /// This object should not be made directly.  Use ConstantChaumPedersenProof::make
    ///
    /// see: https://www.electionguard.vote/spec/0.95.0/5_Ballot_encryption/#proof-of-satisfying-the-selection-limit
    /// </Summary>
    class EG_API ConstantChaumPedersenProof
    {
      public:
        ConstantChaumPedersenProof(const ConstantChaumPedersenProof &other);
        ConstantChaumPedersenProof(const ConstantChaumPedersenProof &&other);
        ConstantChaumPedersenProof(std::unique_ptr<ElementModP> pad,
                                   std::unique_ptr<ElementModP> data,
                                   std::unique_ptr<ElementModQ> challenge,
                                   std::unique_ptr<ElementModQ> response, uint64_t constant);
        ~ConstantChaumPedersenProof();

        ConstantChaumPedersenProof &operator=(ConstantChaumPedersenProof other);
        ConstantChaumPedersenProof &operator=(ConstantChaumPedersenProof &&other);

        /// <Summary>
        /// a in the spec
        /// </Summary>
        ElementModP *getPad() const;

        /// <Summary>
        /// b in the spec
        /// </Summary>
        ElementModP *getData() const;

        /// <Summary>
        /// c in the spec
        /// </Summary>
        ElementModQ *getChallenge() const;

        /// <Summary>
        /// v in the spec
        /// </Summary>
        ElementModQ *getResponse() const;

        /// <Summary>
        /// L in the spec
        /// </Summary>
        uint64_t getConstant() const;

        /// <Summary>
        /// make function for a `ConstantChaumPedersenProof`
        ///
        /// <param name="message"> The ciphertext message</param>
        /// <param name="r"> The nonce used creating the ElGamal ciphertext</param>
        /// <param name="k"> The public key of the election</param>
        /// <param name="seed"> A value used when generating the challenge,
        ///          usually the election extended base hash (𝑄')</param>
        /// <param name="hash_header">Used to generate other random values here</param>
        /// <param name="constant">The constant value to prove</param>
        /// <returns>A unique pointer</returns>
        /// </Summary>
        static std::unique_ptr<ConstantChaumPedersenProof>
        make(const ElGamalCiphertext &message, const ElementModQ &r, const ElementModP &k,
             const ElementModQ &seed, const ElementModQ &hash_header, uint64_t constant,
             bool shouldUsePrecomputedValues = false);

        /// <Summary>
        /// Validates a `ConstantChaumPedersenProof`
        ///
        /// <param name="message"> The ciphertext message</param>
        /// <param name="k"> The public key of the election</param>
        /// <param name="q"> The extended base hash of the election</param>
        /// <returns> True if everything is consistent. False otherwise. </returns>
        /// </Summary>
        bool isValid(const ElGamalCiphertext &message, const ElementModP &k, const ElementModQ &q);

      private:
        class Impl;
#pragma warning(suppress : 4251)
        std::unique_ptr<Impl> pimpl;
    };

    /// <Summary>
    /// The Generic Chaum PedersenProof is a Non-Interactive Zero-Knowledge Proof
    /// that represents the proof of knowing a secret value.
    ///
    /// The proof is used during decryption to prove that the guardains have shared knowledge
    /// of the election secret key. Note the secret key is not computed directly
    /// but instead each guardian computes a share of the secret key and consequently
    /// also computes a sahre of the proof.
    ///
    /// Produces a proof that a given value corresponds to a specific encryption.
    /// computes: 𝑀 =𝐴^𝑠𝑖 mod 𝑝 and 𝐾𝑖 = 𝑔^𝑠𝑖 mod 𝑝
    ///
    /// This object should not be made directly.  Use ChaumPedersenProof::make
    ///
    /// see: TODO: include spec link
    /// </Summary>
    class EG_API ChaumPedersenProof
    {
      public:
        ChaumPedersenProof(const ChaumPedersenProof &other);
        ChaumPedersenProof(const ChaumPedersenProof &&other);
        ChaumPedersenProof(std::unique_ptr<ElGamalCiphertext> commitment,
                           std::unique_ptr<ElementModQ> challenge,
                           std::unique_ptr<ElementModQ> response);
        ChaumPedersenProof(std::unique_ptr<ElementModP> pad, std::unique_ptr<ElementModP> data,
                           std::unique_ptr<ElementModQ> challenge,
                           std::unique_ptr<ElementModQ> response);
        ~ChaumPedersenProof();

        ChaumPedersenProof &operator=(ChaumPedersenProof other);
        ChaumPedersenProof &operator=(ChaumPedersenProof &&other);

        /// <Summary>
        /// a in the spec
        /// </Summary>
        ElementModP *getPad() const;

        /// <Summary>
        /// b in the spec
        /// </Summary>
        ElementModP *getData() const;

        /// <Summary>
        /// c in the spec
        /// </Summary>
        ElementModQ *getChallenge() const;

        /// <Summary>
        /// v in the spec
        /// </Summary>
        ElementModQ *getResponse() const;

        /// <Summary>
        /// make function for a `ChaumPedersenProof`
        ///
        /// Produces a proof that a given value corresponds to a specific encryption.
        /// computes: 𝑀' =𝐴^𝑠 mod 𝑝 and 𝑀 = 𝐵𝑀'^-1 mod 𝑝
        ///
        /// <param name="message"> The ciphertext message</param>
        /// <param name="s">The nonce or secret used to derive the value</param>
        /// <param name="m">The value to prove (usually the partial decryption)</param>
        /// <param name="seed"> A value used when generating the challenge</param>
        /// <param name="hash_header">Used to generate other random values here,
        ///                           usually the hash of election extended base hash (𝑄')</param>
        /// <param name="constant">The constant value to prove</param>
        /// <returns>A unique pointer</returns>
        /// </Summary>
        // static std::unique_ptr<ChaumPedersenProof> make(const ElGamalCiphertext &message,
        //                                                 const ElGamalCiphertext &commitment,
        //                                                 const ElementModP &m,
        //                                                 const ElementModP &elGamalPublicKey,
        //                                                 const ElementModQ &hash_header);

        /// <Summary>
        /// Validates a `ChaumPedersenProof`
        ///
        /// Validates:
        /// - The given value 𝑣 is in the set Z𝑞
        /// - The challenge value 𝑐 satisfies 𝑐 = 𝐻(06,𝑄';𝐾,(𝐴,𝐵),(𝑎,𝑏),𝑀').
        /// - that the equations 𝑎 = 𝑔^𝑣 · 𝐾^𝑐 mod 𝑝 and 𝑏 = 𝐴^𝑣 · 𝑀'^𝑐 mod 𝑝 are satisfied.
        /// <param name="message"> The ciphertext message</param>
        /// <param name="k">The public key corresponding to the private key used to encrypt</param>
        /// <param name="m">The value being checked for validity
        ///                 (usually the accumulated ciphertext decryption, or partial decryption)</param>
        /// <param name="q">The extended base hash of the election</param>
        /// <returns> True if everything is consistent. False otherwise. </returns>
        /// </Summary>
        bool isValid(const ElGamalCiphertext &message, const ElementModP &k, const ElementModP &m,
                     const ElementModQ &q);

      private:
        class Impl;
#pragma warning(suppress : 4251)
        std::unique_ptr<Impl> pimpl;
    };
} // namespace electionguard

#endif /* __ELECTIONGUARD_CPP_CHAUM_PEDERSEN_HPP_INCLUDED__ */
