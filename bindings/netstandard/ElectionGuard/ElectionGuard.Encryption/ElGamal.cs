namespace ElectionGuard
{
    // Declare native types for convenience
    using NativeElGamalCiphertext = NativeInterface.ElGamalCiphertext.ElGamalCiphertextHandle;

    /// <summary>
    /// ElGamal Functions
    /// </summary>
    public static unsafe class Elgamal
    {
        /// <summary>
        /// Encrypts a message with a given random nonce and an ElGamal public key.
        ///
        /// <param name="plaintext"> Message to elgamal_encrypt; must be an integer in [0,Q). </param>
        /// <param name="nonce"> Randomly chosen nonce in [1,Q). </param>
        /// <param name="publicKey"> ElGamal public key. </param>
        /// <returns>A ciphertext tuple.</returns>
        /// </summary>
        public static unsafe ElGamalCiphertext Encrypt(
            ulong plaintext, ElementModQ nonce, ElementModP publicKey)
        {
            var status = NativeInterface.ElGamal.Encrypt(
                    plaintext, nonce.Handle, publicKey.Handle,
                    out NativeElGamalCiphertext ciphertext);
            status.ThrowIfError();
            return new ElGamalCiphertext(ciphertext);
        }
    }
}
