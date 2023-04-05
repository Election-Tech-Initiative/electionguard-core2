
using ElectionGuard.Encryption.Utils.Generators;
using ElectionGuard.ElectionSetup.Tests.Generators;
using ElectionGuard.Decryption.Decryption;

namespace ElectionGuard.Decryption.Tests.Decryption;

// Simple tests using the programmatically generated fake manifest
[TestFixture]
public class TestDecryptWithSharesSimple : DisposableBase
{
    private const int BALLOT_COUNT_CAST = 3;
    private const int BALLOT_COUNT_SPOILED = 2;
    private const int NUMBER_OF_GUARDIANS = 5;
    private const int QUORUM = 3;

    [Test]
    public void Test_Decrypt_Tally_With_All_Guardians_Present_Simple()
    {
        // Arrange
        var data = TestDecryptionData.ConfigureTestCase(
            KeyCeremonyGenerator.GenerateKeyCeremonyData(
            NUMBER_OF_GUARDIANS,
            QUORUM, runKeyCeremony: true),
            ManifestGenerator.GetFakeManifest(),
            BALLOT_COUNT_CAST,
            BALLOT_COUNT_SPOILED);

        var guardians = data.KeyCeremony.Guardians.ToList();

        var mediator = new DecryptionMediator(
            "fake-mediator",
            data.CiphertextTally,
            guardians.Select(i => i.SharePublicKey()).ToList());

        // Act
        foreach (var guardian in guardians)
        {
            var share = guardian.ComputeDecryptionShare(data.CiphertextTally);
            mediator.SubmitShare(share!);
        }
        var plaintextTally = mediator.Decrypt(data.CiphertextTally.TallyId);

        // Assert
        Assert.That(plaintextTally, Is.EqualTo(data.PlaintextTally));
    }

    [Test]
    public void Test_Decrypt_Tally_With_Quorum_Guardians_Present_Simple()
    {
        // Arrange
        var data = TestDecryptionData.ConfigureTestCase(
            KeyCeremonyGenerator.GenerateKeyCeremonyData(
            NUMBER_OF_GUARDIANS,
            QUORUM, runKeyCeremony: true),
            ManifestGenerator.GetFakeManifest(),
            BALLOT_COUNT_CAST,
            BALLOT_COUNT_SPOILED);

        var guardians = data.KeyCeremony.Guardians
                .GetRange(0, QUORUM)
                .ToList();

        var mediator = new DecryptionMediator(
            "fake-mediator",
            data.CiphertextTally,
            guardians.Select(i => i.SharePublicKey()).ToList()
            );

        // Act
        foreach (var guardian in guardians)
        {
            var share = guardian.ComputeDecryptionShare(data.CiphertextTally);
            mediator.SubmitShare(share!);
        }
        var plaintextTally = mediator.Decrypt(data.CiphertextTally.TallyId);

        // Assert
        Assert.That(plaintextTally, Is.EqualTo(data.PlaintextTally));
    }

    // [Test]
    // public void Test_Decrypt_Ballot_With_All_Guardians_Present_Simple()
    // {
    //     // Arrange
    //     var data = TestDecryptionData.ConfigureTestCase(
    //         KeyCeremonyGenerator.GenerateKeyCeremonyData(
    //         NUMBER_OF_GUARDIANS,
    //         QUORUM, runKeyCeremony: true),
    //         ManifestGenerator.GetFakeManifest(),
    //         BALLOT_COUNT_CAST,
    //         BALLOT_COUNT_SPOILED);

    //     var guardians = data.KeyCeremony.Guardians
    //             .ToList();

    //     var mediator = new DecryptionMediator(
    //         "fake-mediator",
    //         data.CiphertextTally,
    //         guardians.Select(i => i.SharePublicKey()).ToList());

    //     // Act
    //     foreach (var guardian in guardians)
    //     {
    //         var share = guardian.ComputeDecryptionShare(data.CiphertextTally);
    //         mediator.SubmitShare(share!);
    //     }
    //     var plaintextTally = mediator.Decrypt(data.CiphertextTally.TallyId);

    //     // Assert
    //     Assert.That(plaintextTally, Is.EqualTo(data.PlaintextTally));
    // }
}
