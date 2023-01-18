namespace ElectionGuard
{
    // Declare native types for convenience
    using NativeElGamalCiphertext = NativeInterface.ElGamalCiphertext.ElGamalCiphertextHandle;
    using NativeHashedElGamalCiphertext = NativeInterface.HashedElGamalCiphertext.HashedElGamalCiphertextHandle;

    /// <summary>
    /// ElGamal Functions
    /// </summary>
    public static class Elgamal
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
                    out NativeElGamalCiphertext ciphertext);
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
        public unsafe static HashedElGamalCiphertext Encrypt(
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
