
using ElectionGuard.Encryption.Utils.Generators;
using ElectionGuard.Encryption.Ballot;

namespace ElectionGuard.Decryption.Tests.Decryption;

[TestFixture]
public class TestDecryptWithSecret : DisposableBase
{
    [Test]
    public void Test_Decrypt_Spoiled_Ballot_With_Nonce()
    {
        // Arrange
        var seed = Constants.ONE_MOD_Q;
        var nonce = Constants.ONE_MOD_Q;
        var manifest = ManifestGenerator.GetFakeManifest();
        var data = ElectionGenerator.GenerateFakeElectionData(1, 1, manifest);

        var ballot = BallotGenerator.GetFakeBallot(data.InternalManifest);

        // Act
        var ciphertext = Encrypt.Ballot(
            ballot, data.InternalManifest, data.Context, seed, nonce);
        var ballotNonce = new ElementModQ(ciphertext.Nonce);
        ciphertext.Spoil();

        var result = ciphertext.IsValid(data.InternalManifest);
        var decrypted = ciphertext.Decrypt(
            data.InternalManifest, data.Context, ballotNonce);

        // Assert
        Assert.That(result.IsValid, Is.True);
        // TODO: Add Equality comparison to PlaintextBallot and re-enable assertion
        // Assert.That(decrypted, Is.EqualTo(ballot));
    }
}
