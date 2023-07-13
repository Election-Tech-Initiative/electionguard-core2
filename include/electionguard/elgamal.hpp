#ifndef __ELECTIONGUARD__CPP_ELGAMAL_HPP_INCLUDED__
#define __ELECTIONGUARD__CPP_ELGAMAL_HPP_INCLUDED__
#include "crypto_hashable.hpp"
#include "export.h"
#include "group.hpp"
#include "precompute_buffers.hpp"

#include <memory>
#include <vector>

namespace electionguard
{
    /// <summary>
    /// An exponential ElGamal keypair.
    /// </summary>
    class EG_API ElGamalKeyPair
    {
      public:
        ElGamalKeyPair(const ElGamalKeyPair &other);
        ElGamalKeyPair(const ElGamalKeyPair &&other);
        ElGamalKeyPair(std::unique_ptr<ElementModQ> secretKey,
                       std::unique_ptr<ElementModP> publicKey);
        ~ElGamalKeyPair();

        ElGamalKeyPair &operator=(ElGamalKeyPair rhs);
        ElGamalKeyPair &operator=(ElGamalKeyPair &&rhs);

        /// <Summary>
        /// The ElGamal Secret Key.
        /// </Summary>
        ElementModQ *getSecretKey();

        /// <Summary>
        /// The ElGamal Public Key.
        /// </Summary>
        ElementModP *getPublicKey();

        /// <Summary>
        /// Make an elgamal keypair from a secret.
        /// </Summary>
        static std::unique_ptr<ElGamalKeyPair> fromSecret(const ElementModQ &secretKey,
                                                          bool isFixedBase = true);

        static std::unique_ptr<ElGamalKeyPair> fromPair(const ElementModQ &secretKey,
                                                        const ElementModP &publicKeyData);

      private:
        class Impl;
#pragma warning(suppress : 4251)
        std::unique_ptr<Impl> pimpl;
    };

    /// <summary>
    /// An "exponential ElGamal ciphertext" (i.e., with the plaintext in the exponent to allow for
    /// homomorphic addition). Create one with `elgamal_encrypt`. Add them with `elgamal_add`.
    /// Decrypt using one of the supplied instance methods.
    /// </summary>
    class EG_API ElGamalCiphertext : public CryptoHashable
    {
      public:
        ElGamalCiphertext(const ElGamalCiphertext &other);
        ElGamalCiphertext(ElGamalCiphertext &&other);
        ElGamalCiphertext(std::unique_ptr<ElementModP> pad, std::unique_ptr<ElementModP> data);
        ~ElGamalCiphertext();

        ElGamalCiphertext &operator=(ElGamalCiphertext rhs);
        ElGamalCiphertext &operator=(ElGamalCiphertext &&rhs);
        bool operator==(const ElGamalCiphertext &other);
        bool operator!=(const ElGamalCiphertext &other);

        /// <Summary>
        /// The pad value also referred to as A, a, 𝑎, or alpha in the spec.
        /// </Summary>
        ElementModP *getPad();

        /// <Summary>
        /// The pad value also referred to as A, a, 𝑎, or alpha in the spec.
        /// </Summary>
        ElementModP *getPad() const;

        /// <Summary>
        /// The data value also referred to as B, b, 𝛽, or beta in the spec.
        /// </Summary>
        ElementModP *getData();

        /// <Summary>
        /// The data value also referred to as B, b, 𝛽, or beta in the spec.
        /// </Summary>
        ElementModP *getData() const;

        virtual std::unique_ptr<ElementModQ> crypto_hash() override;
        virtual std::unique_ptr<ElementModQ> crypto_hash() const override;

        /// <Summary>
        /// Make an ElGamal Ciphertext from the given pad and data
        /// </Summary>
        static std::unique_ptr<ElGamalCiphertext> make(const ElementModP &pad,
                                                       const ElementModP &data);

        /// <summary>
        /// Homomorphically accumulates other ElGamal ciphertext by pairwise multiplication
        /// and returns the result without modifying the original.
        /// </summary>
        std::unique_ptr<ElGamalCiphertext> elgamalAdd(const ElGamalCiphertext &b);

        /// <summary>
        /// Homomorphically accumulates another ElGamal ciphertext by pairwise multiplication
        /// and returns the result without modifying the original.
        /// </summary>
        std::unique_ptr<ElGamalCiphertext>
        elgamalAdd(const std::vector<std::reference_wrapper<ElGamalCiphertext>> &ciphertexts);

        /// <Summary>
        /// Decrypts an ElGamal ciphertext with a "known product" (the blinding factor used in the encryption).
        /// Calculates 𝑀=𝐵⁄(∏𝑀𝑖) mod 𝑝.
        ///
        /// <param name="product">The known product (blinding factor).</param>
        /// <returns>An exponentially encoded plaintext message.</returns
        /// </Summary>
        uint64_t decrypt(const ElementModP &product);

        /// <Summary>
        /// Decrypts an ElGamal ciphertext with a "known product" (the blinding factor used in the encryption).
        ///
        /// <param name="product">The known product (blinding factor).</param>
        /// <returns>An exponentially encoded plaintext message.</returns
        /// </Summary>
        uint64_t decrypt(const ElementModP &product) const;

        /// <Summary>
        /// Decrypt the ciphertext directly using the provided secret key.
        ///
        /// This is a convenience accessor useful for some use cases.
        /// This method should not be used by consumers operating in live secret ballot elections.
        ///
        /// <param name="secretKey">The corresponding ElGamal secret key.</param>
        /// <returns>An exponentially encoded plaintext message.</returns
        /// </Summary>
        uint64_t decrypt(const ElementModQ &secretKey);

        /// <Summary>
        /// Decrypt the ciphertext directly using the provided secret key.
        ///
        /// This is a convenience accessor useful for some use cases.
        /// This method should not be used by consumers operating in live secret ballot elections.
        ///
        /// <param name="secretKey">The corresponding ElGamal secret key.</param>
        /// <returns>An exponentially encoded plaintext message.</returns
        /// </Summary>
        uint64_t decrypt(const ElementModQ &secretKey) const;

        /// <Summary>
        /// Decrypt an ElGamal ciphertext using a known nonce and the ElGamal public key.
        ///
        /// <param name="publicKey">The corresponding ElGamal Public Key</param>
        /// <param name="nonce">The secret nonce used to create the ciphertext.</param>
        /// <returns>An exponentially encoded plaintext message.</returns
        /// </Summary>
        uint64_t decrypt(const ElementModP &publicKey, const ElementModQ &nonce);

        /// <Summary>
        /// Decrypt an ElGamal ciphertext using a known nonce and the ElGamal public key.
        ///
        /// <param name="publicKey">The corresponding ElGamal Public Key</param>
        /// <param name="nonce">The secret nonce used to create the ciphertext.</param>
        /// <returns>An exponentially encoded plaintext message.</returns
        /// </Summary>
        uint64_t decrypt(const ElementModP &publicKey, const ElementModQ &nonce) const;

        /// <Summary>
        /// Partially Decrypts an ElGamal ciphertext with a known ElGamal secret key.
        /// 𝑀_i = 𝐴^𝑠𝑖 mod 𝑝 in the spec
        ///
        /// <param name="secretKey">The corresponding ElGamal secret key.</param>
        /// <returns>A partial decryption of the plaintext value</returns
        /// </Summary>
        std::unique_ptr<ElementModP> partialDecrypt(const ElementModQ &secretKey);

        /// <Summary>
        /// Partially Decrypts an ElGamal ciphertext with a known ElGamal secret key.
        /// 𝑀_i = 𝐴^𝑠𝑖 mod 𝑝 in the spec
        ///
        /// <param name="secretKey">The corresponding ElGamal secret key.</param>
        /// <returns>A partial decryption of the plaintext value</returns
        /// </Summary>
        std::unique_ptr<ElementModP> partialDecrypt(const ElementModQ &secretKey) const;

        /// <Summary>
        /// Clone the value by making a deep copy.
        /// </Summary>
        std::unique_ptr<ElGamalCiphertext> clone() const;

      private:
        class Impl;
#pragma warning(suppress : 4251)
        std::unique_ptr<Impl> pimpl;
    };

    /// <summary>
    /// Encrypts a message with a given random nonce and an ElGamal public key.
    ///
    /// <param name="m"> Message to elgamal_encrypt; must be an integer in [0,Q). </param>
    /// <param name="nonce"> Randomly chosen nonce in [1,Q). </param>
    /// <param name="publicKey"> ElGamal public key. </param>
    /// <returns>A ciphertext tuple.</returns>
    /// </summary>
    EG_API std::unique_ptr<ElGamalCiphertext>
    elgamalEncrypt(const uint64_t m, const ElementModQ &nonce, const ElementModP &publicKey);

    /// <summary>
    /// Encrypts a message with given precomputed values (two triples and a quadruple).
    /// However, only the first triple is used in this function.
    ///
    /// <param name="m"> Message to elgamal_encrypt; must be an integer in [0,Q). </param>
    /// <param name="precomputedTwoTriplesAndAQuad"> Precomputed two triples and a quad. </param>
    /// <returns>A ciphertext tuple.</returns>
    /// </summary>
    EG_API std::unique_ptr<ElGamalCiphertext>
    elgamalEncrypt(uint64_t m, const TwoTriplesAndAQuadruple &precomputedValues);

    /// <summary>
    /// Homomorphically accumulates one or more ElGamal ciphertexts by pairwise multiplication.
    /// The exponents of vote counters will add.
    /// </summary>
    EG_API std::unique_ptr<ElGamalCiphertext>
    elgamalAdd(const std::vector<std::reference_wrapper<ElGamalCiphertext>> &ciphertexts);

    /// <summary>
    /// Homomorphically accumulates one or more ElGamal ciphertexts by pairwise multiplication.
    /// The exponents of vote counters will add.
    /// </summary>
    EG_API std::unique_ptr<ElGamalCiphertext> elgamalAdd(const ElGamalCiphertext &a,
                                                         const ElGamalCiphertext &b);

    /// <summary>
    /// A "Hashed ElGamal Ciphertext" as specified as the Auxiliary Encryption in
    /// the ElectionGuard specification. The tuple g^r mod p concatenated with
    /// K^r mod p are used to feed into a hash function to generate a main (session) key
    /// from which other keys derive to perform XOR encryption and to MAC the
    /// result. Create one with `hashedElgamalEncrypt`. Decrypt using one of the
    /// 'decrypt' methods.
    /// </summary>
    class EG_API HashedElGamalCiphertext : public CryptoHashable
    {
      public:
        HashedElGamalCiphertext(const HashedElGamalCiphertext &other);
        HashedElGamalCiphertext(HashedElGamalCiphertext &&other);
        HashedElGamalCiphertext(std::unique_ptr<ElementModP> pad, std::vector<uint8_t> data,
                                std::vector<uint8_t> mac);
        ~HashedElGamalCiphertext();

        HashedElGamalCiphertext &operator=(HashedElGamalCiphertext rhs);
        HashedElGamalCiphertext &operator=(HashedElGamalCiphertext &&rhs);
        bool operator==(const HashedElGamalCiphertext &other);
        bool operator!=(const HashedElGamalCiphertext &other);

        /// <Summary>
        /// The g^r mod p value also referred to as pad in the code and
        /// c0, 𝑎, or alpha in the spec.
        /// </Summary>
        ElementModP *getPad();

        /// <Summary>
        /// The g^r mod p value also referred to as pad in the code and
        /// c0, 𝑎, or alpha in the spec.
        /// </Summary>
        ElementModP *getPad() const;

        /// <Summary>
        /// The vector of encrypted ciphertext bytes. Referred to as c1
        /// in the spec.
        /// </Summary>
        std::vector<uint8_t> getData();

        /// <Summary>
        /// The vector of encrypted ciphertext bytes. Referred to as c1
        /// in the spec.
        /// </Summary>
        std::vector<uint8_t> getData() const;

        /// <Summary>
        /// The vector of MAC bytes. Referred to as c2 in the spec.
        /// </Summary>
        std::vector<uint8_t> getMac();

        /// <Summary>
        /// The vector of MAC bytes. Referred to as c2 in the spec.
        /// </Summary>
        std::vector<uint8_t> getMac() const;

        virtual std::unique_ptr<ElementModQ> crypto_hash() override;
        virtual std::unique_ptr<ElementModQ> crypto_hash() const override;

        static std::unique_ptr<HashedElGamalCiphertext>
        make(const ElementModP &pad, std::vector<uint8_t> data, std::vector<uint8_t> mac);

        /// <summary>
        /// Decrypts ciphertext with the Auxiliary Encryption method (as specified in the
        /// ElectionGuard specification) given a random nonce, an ElGamal public key,
        /// and an encryption seed. The encrypt may be called to look for padding to
        /// verify and remove, in this case the plaintext will be smaller than
        /// the ciphertext, or not to look for padding in which case the
        /// plaintext will be the same size as the ciphertext.
        ///
        /// <param name="publicKey">ElGamal Public Key</param>
        /// <param name="secretKey">ElGamal secret key or nonce</param>
        /// <param name="hashPrefix">A prefix value for the hash used to create the session key.</param>
        /// <param name="seed">An encruption seed used to generate the session key.</param>
        /// <param name="expectPadding">Indicates if padding should be removed from the decrypted value.</param>
        /// <returns>A plaintext vector.</returns>
        /// </summary>
        std::vector<uint8_t> decrypt(const ElementModP &publicKey, const ElementModQ &secretKey,
                                     const std::string &hashPrefix, const ElementModQ &seed,
                                     bool expectPadding);

        /// <Summary>
        /// Partially Decrypts an ElGamal ciphertext with a known ElGamal secret key.
        /// 𝑀_i = C0^P𝑖 mod 𝑝 in the spec
        ///
        /// <param name="secretKey">The corresponding ElGamal secret key.</param>
        /// <returns>A partial decryption of the plaintext value</returns
        /// </Summary>
        std::unique_ptr<ElementModP> partialDecrypt(const ElementModQ &secretKey);

        /// <Summary>
        /// Partially Decrypts an ElGamal ciphertext with a known ElGamal secret key.
        /// 𝑀_i = C0^P𝑖 mod 𝑝 in the spec
        ///
        /// <param name="secretKey">The corresponding ElGamal secret key.</param>
        /// <returns>A partial decryption of the plaintext value</returns
        /// </Summary>
        std::unique_ptr<ElementModP> partialDecrypt(const ElementModQ &secretKey) const;

        /// <Summary>
        /// Clone the value by making a deep copy.
        /// </Summary>
        std::unique_ptr<HashedElGamalCiphertext> clone() const;

      private:
        class Impl;
#pragma warning(suppress : 4251)
        std::unique_ptr<Impl> pimpl;
    };

    /// <summary>
    /// Encrypts a message with the Auxiliary Encryption method (as specified in the
    /// ElectionGuard specification) given a random nonce, an ElGamal public key,
    /// and an encryption seed.
    ///
    /// The encrypt may be called to apply padding. If
    /// padding is to be applied then the max_len parameter may be used with
    /// any of the HASHED_CIPHERTEXT_PADDED_DATA_SIZE enumeration values.
    /// This value indicates the maximum length of the plaintext that may be
    /// encrypted. The padding scheme applies two bytes for length of padding
    /// plus padding bytes.
    ///
    /// If allow_truncation parameter is set to
    /// true then if the message parameter data is longer than
    /// max_len then it will be truncated to max_len.
    /// If the allow_truncation parameter
    /// is set to false then if the message parameter data is longer than
    /// max_len then an exception will be thrown.
    ///
    /// <param name="message">Message to hashed elgamal encrypt.</param>
    /// <param name="nonce">Randomly chosen nonce in [1,Q).</param>
    /// <param name="hashPrefix">A prefix value for the hash used to create the session key.</param>
    /// <param name="publicKey">ElGamal public key.</param>
    /// <param name="seed">Hash of the ballot description.</param>
    /// <param name="max_len">Indicates the maximum length of plaintext,
    ///                       must be one of the `HASHED_CIPHERTEXT_PADDED_DATA_SIZE`
    ///                       enumeration values.
    /// </param>
    /// <param name="allow_truncation">Truncates data to the max_len if set to true.
    /// </param>
    /// <param name="shouldUsePrecomputedValues">If true, the function will attempt
    ///                                          to use a precomputed value form the precompute buffer
    /// </param>
    /// <returns>A ciphertext triple.</returns>
    /// </summary>
    EG_API std::unique_ptr<HashedElGamalCiphertext>
    hashedElgamalEncrypt(std::vector<uint8_t> message, const ElementModQ &nonce,
                         const std::string &hashPrefix, const ElementModP &publicKey,
                         const ElementModQ &seed, HASHED_CIPHERTEXT_PADDED_DATA_SIZE max_len,
                         bool allowTruncation, bool usePrecompute = false);

    /// <summary>
    /// Encrypts a message with the Auxiliary Encryption method (as specified in the
    /// ElectionGuard specification) given a random nonce, an ElGamal public key,
    /// and an encryption seed.
    ///
    /// the `message` parameter must be a multiple of the block length (32)
    /// and the ciphertext will be the same size.
    ///
    /// <param name="message">Message to hashed elgamal encrypt.</param>
    /// <param name="nonce">Randomly chosen nonce in [1,Q).</param>
    /// <param name="hashPrefix">A prefix value for the hash used to create the session key.</param>
    /// <param name="publicKey">ElGamal public key.</param>
    /// <param name="seed">A seed value used to create the session key.</param>
    /// <param name="shouldUsePrecomputedValues">If true, the function will attempt
    ///                                          to use a precomputed value form the precompute buffer
    /// </param>
    /// <returns>A ciphertext triple.</returns>
    /// </summary>
    EG_API std::unique_ptr<HashedElGamalCiphertext>
    hashedElgamalEncrypt(std::vector<uint8_t> message, const ElementModQ &nonce,
                         const std::string &hashPrefix, const ElementModP &publicKey,
                         const ElementModQ &seed, bool usePrecompute = false);

} // namespace electionguard

#endif /* __ELECTIONGUARD__CPP_ELGAMAL_HPP_INCLUDED__ */
