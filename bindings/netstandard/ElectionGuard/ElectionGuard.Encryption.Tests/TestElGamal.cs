using System.Collections.Generic;
using NUnit.Framework;

namespace ElectionGuard.Encryption.Tests
{
    [TestFixture]
    public class TestElGamal
    {
        [Test]
        public void Test_Elgamal_Encrypt_Simple()
        {
            // Arrange
            var nonce = Constants.ONE_MOD_Q;
            var secret = Constants.TWO_MOD_Q;
            var keyPair = ElGamalKeyPair.FromSecret(secret);
            var publicKey = keyPair.PublicKey;
            const ulong vote = 1UL;

            // Act
            var ciphertext = ElGamal.Encrypt(vote, nonce, publicKey);
            var plaintext = ciphertext.Decrypt(keyPair.SecretKey);

            // Assert
            Assert.That(plaintext == vote);

            // Cleanup
            ciphertext.Dispose();
        }

        [Test]
        public void Test_Elgamal_Add_Simple()
        {
            // Arrange
            var nonce = Constants.ONE_MOD_Q;
            var secret = Constants.TWO_MOD_Q;
            var keyPair = ElGamalKeyPair.FromSecret(secret);
            var publicKey = keyPair.PublicKey;
            const ulong vote = 1UL;

            // Act
            var firstCiphertext = ElGamal.Encrypt(vote, nonce, publicKey);
            var secondCiphertext = ElGamal.Encrypt(vote, nonce, publicKey);
            var result = ElGamal.Add(new List<ElGamalCiphertext> { firstCiphertext, secondCiphertext });
            var plaintext = result.Decrypt(keyPair.SecretKey);

            // Assert
            Assert.That(plaintext == vote + vote);

            // Cleanup
            firstCiphertext.Dispose();
            secondCiphertext.Dispose();
            result.Dispose();
        }
    }
}
