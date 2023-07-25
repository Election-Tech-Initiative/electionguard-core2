using NUnit.Framework;

namespace ElectionGuard.Encryption.Tests
{
    [TestFixture]
    public class TestChaumPedersen
    {
        [Test]
        public void Test_DisjunctiveChaumPedersen()
        {
            var nonce = new ElementModQ(Constants.ONE_MOD_Q);
            var keyPair = ElGamalKeyPair.FromSecret(Constants.TWO_MOD_Q);
            const ulong vote = 0UL;
            var ciphertext = ElGamal.Encrypt(vote, nonce, keyPair.PublicKey);

            var proof = new DisjunctiveChaumPedersenProof(
                ciphertext, nonce, keyPair.PublicKey, new ElementModQ(Constants.ONE_MOD_Q), vote);

            Assert.That(proof.IsValid(ciphertext, keyPair.PublicKey, Constants.ONE_MOD_Q));
        }

        [Test]
        public void Test_DisjunctiveChaumPedersen_deterministic()
        {
            var nonce = Constants.ONE_MOD_Q;
            var seed = Constants.TWO_MOD_Q;
            var keyPair = ElGamalKeyPair.FromSecret(Constants.TWO_MOD_Q);
            const ulong vote = 0UL;
            var ciphertext = ElGamal.Encrypt(vote, nonce, keyPair.PublicKey);

            var proof = new DisjunctiveChaumPedersenProof(
                ciphertext, nonce, keyPair.PublicKey, Constants.ONE_MOD_Q, seed, vote);

            Assert.That(proof.IsValid(ciphertext, keyPair.PublicKey, Constants.ONE_MOD_Q));
        }

        [Test]
        public void Test_RangedChaumPedersen_Deterministic()
        {
            var nonce = Constants.ONE_MOD_Q;
            var seed = Constants.TWO_MOD_Q;
            var keyPair = ElGamalKeyPair.FromSecret(Constants.TWO_MOD_Q);
            const ulong vote = 0UL;
            const ulong max = 1UL;
            var ciphertext = ElGamal.Encrypt(vote, nonce, keyPair.PublicKey);

            var proof = new RangedChaumPedersenProof(
                ciphertext, Constants.ONE_MOD_Q, vote, max, keyPair.PublicKey, Constants.ONE_MOD_Q, seed);

            Assert.That(proof.IsValid(ciphertext, keyPair.PublicKey, Constants.ONE_MOD_Q, out var _));
        }
    }
}
