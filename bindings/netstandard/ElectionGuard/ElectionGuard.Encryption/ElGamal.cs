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
            if (ciphertext.IsInvalid)
            {
                return null;
            }
            return new ElGamalCiphertext(ciphertext);
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
                    nativeCiphertexts.ToArray(), (ulong)nativeCiphertexts.Count,
                    out var ciphertext);
            status.ThrowIfError();
            if (ciphertext.IsInvalid)
            {
                return null;
            }
            return new ElGamalCiphertext(ciphertext);
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
            if (ciphertext.IsInvalid)
            {
                return null;
            }
            return new ElGamalCiphertext(ciphertext);
        }
    }

    /// <summary>
    /// Class to hold the factory method to create HashedElGamalCiphertext
    /// </summary>
    public static class HashedElgamal
    {
        public static unsafe HashedElGamalCiphertext Encrypt(
            ElementModQ coordinate, ElementModQ nonce, ElementModP publicKey, ElementModQ seed)
        {
            if (coordinate == null || !coordinate.IsInBounds())
            {
                throw new ArgumentNullException(nameof(coordinate));
            }
            var data = coordinate.ToBytes();
            return Encrypt(data, (ulong)data.Length, nonce, publicKey, seed);
        }

        /// <summary>
        /// Encrypts a message with a given random nonce and an ElGamal public key.
        ///
        /// <param name="data"> Message to elgamal_encrypt; must be an integer in [0,Q). </param>
        /// <param name="length"> Length of the data array </param>
        /// <param name="nonce"> Randomly chosen nonce in [1,Q). </param>
        /// <param name="publicKey"> ElGamal public key. </param>
        /// <param name="seed"> ElGamal seed. </param>
        /// <returns>A ciphertext tuple.</returns>
        /// </summary>
        public static unsafe HashedElGamalCiphertext Encrypt(
            byte[] data, ulong length, ElementModQ nonce, ElementModP publicKey, ElementModQ seed)
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
                    pointer, length, nonce.Handle, publicKey.Handle, seed.Handle,
                    out var ciphertext);
                status.ThrowIfError();
                if (ciphertext.IsInvalid)
                {
                    return null;
                }
                return new HashedElGamalCiphertext(ciphertext);
            }
        }
    }
}
