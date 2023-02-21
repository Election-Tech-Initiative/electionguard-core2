using ElectionGuard.Decryption.Tally;

namespace ElectionGuard.Decryption.Tests;

public class Tests
{
    private readonly ElementModQ nonce = Constants.ONE_MOD_Q;
    private readonly ElementModQ secret = Constants.TWO_MOD_Q;

    ElGamalKeyPair keyPair = default!;

    [SetUp]
    public void Setup()
    {
        keyPair = ElGamalKeyPair.FromSecret(secret);
    }

    [Test]
    public void Test_ElGamalAccumulate_Decrypts_With_Secret()
    {
        // Arrange
        const ulong vote = 1UL;
        const ulong count = 4;
        var publicKey = keyPair.PublicKey;
        var ciphertexts = Enumerable.Range(0, (int)count)
            .Select(i => ElGamal.Encrypt(vote, nonce, publicKey));

        // Act
        var subject = new CiphertextTallySelection(
            "some_object_id", 0, Constants.ONE_MOD_Q);
        _ = subject.ElGamalAccumulate(ciphertexts);

        // Assert
        var plaintext = subject.Ciphertext.Decrypt(keyPair.SecretKey);
        Assert.That(plaintext, Is.EqualTo(vote * count));

        // Cleanup
        foreach (var ciphertext in ciphertexts)
        {
            ciphertext.Dispose();
        }
        subject.Dispose();
    }
}
