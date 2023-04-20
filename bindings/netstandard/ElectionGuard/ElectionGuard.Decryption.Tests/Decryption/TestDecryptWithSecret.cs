
using ElectionGuard.Encryption.Utils.Generators;
using ElectionGuard.Encryption.Ballot;

namespace ElectionGuard.Decryption.Tests.Decryption;

[TestFixture]
public class TestDecryptWithSecret : DisposableBase
{
    [Test]
    public void Test_Decrypt_Challenged_Ballot_With_Nonce()
    {
        // Arrange
        using var seed = Constants.ONE_MOD_Q;
        using var nonce = Constants.ONE_MOD_Q;
        using var manifest = ManifestGenerator.GetFakeManifest();
        using var data = ElectionGenerator.GenerateFakeElectionData(1, 1, manifest);

        using var ballot = BallotGenerator.GetFakeBallot(data.InternalManifest);

        // Act
        using var ciphertext = Encrypt.Ballot(
            ballot, data.InternalManifest, data.Context, seed, nonce);
        using var ballotNonce = new ElementModQ(ciphertext.Nonce);
        ciphertext.Challenge();

        var result = ciphertext.IsValid(data.InternalManifest);
        using var decrypted = ciphertext.Decrypt(
            data.InternalManifest, data.Context, ballotNonce);

        // Assert
        Assert.That(result.IsValid, Is.True);
        // TODO: Add Equality comparison to PlaintextBallot and re-enable assertion
        // Assert.That(decrypted, Is.EqualTo(ballot));
    }
}
