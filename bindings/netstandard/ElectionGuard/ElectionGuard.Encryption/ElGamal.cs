using System;
using System.Collections.Generic;
using System.Linq;

namespace ElectionGuard
{
    /// <summary>
    /// ElGamal Functions
    /// </summary>
    public static class ElGamal
    {
        /// <summary>
        /// Encrypts a message with a given random nonce and an ElGamal public key.
        ///
        /// <param name="plaintext"> Message to elgamal_encrypt; must be an integer in [0,Q). </param>
        /// <param name="nonce"> Randomly chosen nonce in [1,Q). </param>
        /// <param name="publicKey"> ElGamal public key. </param>
        /// <returns>A ciphertext tuple.</returns>
        /// </summary>
        public static ElGamalCiphertext Encrypt(
            ulong plaintext, ElementModQ nonce, ElementModP publicKey)
        {
            var status = NativeInterface.ElGamal.Encrypt(
                    plaintext, nonce.Handle, publicKey.Handle,
                    out var ciphertext);
            status.ThrowIfError();
            return ciphertext.IsInvalid
                ? null : new ElGamalCiphertext(ciphertext);
        }

        /// <summary>
        /// Homomorphically accumulates one or more ElGamal ciphertexts by pairwise multiplication.
        /// The exponents of vote counters will add.
        ///
        /// <param name="ciphertexts"> A collection of Ciphertexts to combine</param>
        /// <returns>A ciphertext tuple.</returns>
        /// </summary>
        public static ElGamalCiphertext Add(
            IEnumerable<ElGamalCiphertext> ciphertexts)
        {
            var nativeCiphertexts =
                ciphertexts.Select(c => c.Handle.Ptr).ToList();

            var status = NativeInterface.ElGamal.Add(
                    nativeCiphertexts.ToArray(),
                    (ulong)nativeCiphertexts.Count,
                    out var ciphertext);
            status.ThrowIfError();
            return ciphertext.IsInvalid
                ? null
                : new ElGamalCiphertext(ciphertext);
        }

        public static ElGamalCiphertext Add(
            params ElGamalCiphertext[] ciphertexts)
        {
            return Add(ciphertexts.AsEnumerable());
        }

        public static ElGamalCiphertext Add(
           ElGamalCiphertext a, ElGamalCiphertext b)
        {
            var status = NativeInterface.ElGamal.Add(
                    a.Handle, b.Handle,
                    out var ciphertext);
            status.ThrowIfError();
            return ciphertext.IsInvalid
                ? null
                : new ElGamalCiphertext(ciphertext);
        }
    }

    /// <summary>
    /// Class to hold the factory method to create HashedElGamalCiphertext
    /// </summary>
    public static class HashedElgamal
    {
        public static unsafe HashedElGamalCiphertext Encrypt(
            ElementModQ coordinate, ElementModQ nonce,
            string hashPrefix, ElementModP publicKey, ElementModQ seed)
        {
            if (coordinate == null || !coordinate.IsInBounds())
            {
                throw new ArgumentNullException(nameof(coordinate));
            }
            var data = coordinate.ToBytes();
            return Encrypt(
                data, (ulong)data.Length, nonce,
                hashPrefix, publicKey, seed, false);
        }

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
        public static unsafe HashedElGamalCiphertext Encrypt(
            byte[] data, ulong length, ElementModQ nonce,
            string hashPrefix, ElementModP publicKey, ElementModQ seed,
            uint maxLength, bool allowTruncation, bool usePrecompute)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (nonce == null || !nonce.IsInBounds())
            {
                throw new ArgumentNullException(nameof(nonce));
            }
            if (publicKey == null || !publicKey.IsInBounds())
            {
                throw new ArgumentNullException(nameof(publicKey));
            }
            if (seed == null || !seed.IsInBounds())
            {
                throw new ArgumentNullException(nameof(seed));
            }

            // TODO: HACK: ISSUE #358: the hashed elgamal process uses a 32 byte block length
            // but reserves a few bytes for encoding. the indiator is sizeof(uint16_t)
            // in the c/c++ constants.h file but we need to guarantee the width
            // for now, we just make sure the maxLength is within an acceptable range
            // and it is up to the caller to ensure the proper integer value is passed
            // for their system. This will be fixed as part of ISSUE #358
            if (maxLength > 512)
            {
                throw new ArgumentOutOfRangeException(nameof(maxLength));
            }

            fixed (byte* pointer = data)
            {
                var status = NativeInterface.HashedElGamal.Encrypt(
                    pointer, length, nonce.Handle, hashPrefix,
                    publicKey.Handle, seed.Handle,
                    maxLength, allowTruncation, usePrecompute,
                    out var ciphertext);
                status.ThrowIfError();
                return ciphertext.IsInvalid
                    ? null : new HashedElGamalCiphertext(ciphertext);
            }
        }

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
        public static unsafe HashedElGamalCiphertext Encrypt(
            byte[] data, ulong length, ElementModQ nonce,
            string hashPrefix, ElementModP publicKey, ElementModQ seed,
            bool usePrecompute)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (nonce == null || !nonce.IsInBounds())
            {
                throw new ArgumentNullException(nameof(nonce));
            }
            if (publicKey == null || !publicKey.IsInBounds())
            {
                throw new ArgumentNullException(nameof(publicKey));
            }
            if (seed == null || !seed.IsInBounds())
            {
                throw new ArgumentNullException(nameof(seed));
            }

            fixed (byte* pointer = data)
            {
                var status = NativeInterface.HashedElGamal.Encrypt(
                    pointer, length, nonce.Handle, hashPrefix,
                    publicKey.Handle, seed.Handle, usePrecompute,
                    out var ciphertext);
                status.ThrowIfError();
                return ciphertext.IsInvalid
                    ? null : new HashedElGamalCiphertext(ciphertext);
            }
        }
    }
}
