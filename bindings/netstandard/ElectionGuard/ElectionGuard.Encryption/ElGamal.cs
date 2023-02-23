using System;
using System.Collections.Generic;
using System.Linq;
// Declare native types for convenience
using NativeElGamalCiphertext = ElectionGuard.NativeInterface.ElGamalCiphertext.ElGamalCiphertextHandle;
using NativeHashedElGamalCiphertext = ElectionGuard.NativeInterface.HashedElGamalCiphertext.HashedElGamalCiphertextHandle;

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
                ciphertexts.Select(c => c.Handle.Ptr);

            var status = NativeInterface.ElGamal.Add(
                    nativeCiphertexts.ToArray(), (ulong)nativeCiphertexts.Count(),
                    out var ciphertext);

            status.ThrowIfError();
            return new ElGamalCiphertext(ciphertext);
        }
    }

    /// <summary>
    /// Class to hold the factory method to create HashedElGamalCiphertext
    /// </summary>
    public static class HashedElgamal
    {
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
            fixed (byte* pointer = data)
            {
                var status = NativeInterface.HashedElGamal.Encrypt(
                    pointer, length, nonce.Handle, publicKey.Handle, seed.Handle,
                    out NativeHashedElGamalCiphertext ciphertext);
                status.ThrowIfError();
                return new HashedElGamalCiphertext(ciphertext);
            }
        }
    }
}
