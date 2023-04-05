
using ElectionGuard.Encryption.Utils.Generators;
using ElectionGuard.ElectionSetup.Tests.Generators;
using ElectionGuard.Decryption.Decryption;
using ElectionGuard.Encryption.Ballot;

namespace ElectionGuard.Decryption.Tests.Decryption;

[TestFixture]
public class TestDecryptWithSecret : DisposableBase
{
    private const int BALLOT_COUNT_CAST = 30;
    private const int BALLOT_COUNT_SPOILED = 2;
    private const int NUMBER_OF_GUARDIANS = 5;
    private const int QUORUM = 3;

    private TestElectionData Data = default!;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        Data = ElectionGenerator.GenerateFakeElectionData();
    }

    [Test]
    public void Test_Decrypt_Spoiled_Ballot_With_Secret()
    {
        // Arrange
        var data = ElectionGenerator.GenerateFakeElectionData();
        var mediator = new EncryptionMediator(
            data.InternalManifest, data.Context, data.Device);

        var ballot = BallotGenerator.GetFakeBallot(data.InternalManifest);

        // Act
        var ciphertext = mediator.Encrypt(ballot);
        ciphertext.Spoil();
        var result = ciphertext.IsValid(data.InternalManifest);
        var decrypted = ciphertext.Decrypt(data.InternalManifest, data.KeyPair.SecretKey);

        // Assert
        Assert.That(result.IsValid, Is.True);
        // TODO: assert that the decrypted ballot is equal to the original ballot

    }
}
