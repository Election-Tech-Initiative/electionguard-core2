using ElectionGuard.Decryption.Tally;

namespace ElectionGuard.Decryption.Tests.Tally;

public class TestCiphertextTallySelection
{
    private readonly ElementModQ nonce = Constants.ONE_MOD_Q;
    private readonly ElementModQ secret = Constants.TWO_MOD_Q;

    private ElGamalKeyPair keyPair = default!;

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
            .Select(_ => ElGamal.Encrypt(vote, nonce, publicKey)).ToList();

        // Act
        var subject = new CiphertextTallySelection(
            "some_object_id", 0, Constants.ONE_MOD_Q);
        _ = subject.Accumulate(ciphertexts);

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
