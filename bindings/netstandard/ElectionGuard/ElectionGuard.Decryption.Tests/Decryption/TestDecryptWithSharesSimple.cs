
using ElectionGuard.Encryption.Utils.Generators;
using ElectionGuard.Decryption.Tests.Tally;

namespace ElectionGuard.Decryption.Tests.Decryption;

// Simple tests using the programmatically generated fake manifest
[TestFixture]
public class TestDecryptWithSharesSimple : DisposableBase
{
    private const int BALLOT_COUNT_CAST = 2;
    private const int BALLOT_COUNT_CHALLENGED = 1;
    private const int BALLOT_COUNT_SPOILED = 1;
    private const int NUMBER_OF_GUARDIANS = 3;
    private const int QUORUM = 2;

    [Test]
    public void Test_Decrypt_With_All_Guardians_Present_Simple()
    {
        // Arrange
        using var data = TestDecryptionData.ConfigureTestCase(
            KeyCeremonyGenerator.GenerateKeyCeremonyData(
            NUMBER_OF_GUARDIANS,
            QUORUM, runKeyCeremony: true),
            ManifestGenerator.GetFakeManifest(),
            BALLOT_COUNT_CAST,
            BALLOT_COUNT_CHALLENGED,
            BALLOT_COUNT_SPOILED);

        using var mediator = new DecryptionMediator(
            "fake-mediator",
            data.CiphertextTally,
            data.KeyCeremony.Guardians.Select(i => i.SharePublicKey()).ToList());

        // Act
        var guardians = data.KeyCeremony.Guardians
            .ToList();
        var result = mediator.RunDecryptionProcess(data, guardians);

        // Assert
        var plaintextChallengedBallots = data.PlaintextBallots
            .Where(i => data.CiphertextTally.ChallengedBallotIds.Contains(i.ObjectId))
            .Select(i => i.ToTallyBallot(data.CiphertextTally)).ToList();

        Assert.That(result.Tally, Is.EqualTo(data.PlaintextTally));
        Assert.That(result.ChallengedBallots!.Count, Is.EqualTo(plaintextChallengedBallots.Count));
        Assert.That(result.ChallengedBallots, Is.EqualTo(plaintextChallengedBallots));
    }

    [Test] // Category("SingleTest")
    public void Test_Decrypt_With_Quorum_Guardians_Present_Simple()
    {
        // Arrange
        using var data = TestDecryptionData.ConfigureTestCase(
            KeyCeremonyGenerator.GenerateKeyCeremonyData(
            NUMBER_OF_GUARDIANS,
            QUORUM, runKeyCeremony: true),
            ManifestGenerator.GetFakeManifest(),
            BALLOT_COUNT_CAST,
            BALLOT_COUNT_CHALLENGED,
            BALLOT_COUNT_SPOILED);

        using var mediator = new DecryptionMediator(
            "fake-mediator",
            data.CiphertextTally,
            data.KeyCeremony.Guardians.Select(i => i.SharePublicKey()).ToList()
            );

        // Act
        var guardians = data.KeyCeremony.Guardians
                .GetRange(0, QUORUM)
                .ToList();
        var result = mediator.RunDecryptionProcess(data, guardians);

        // Assert
        var plaintextChallengedBallots = data.PlaintextBallots
            .Where(i => data.CiphertextTally.ChallengedBallotIds.Contains(i.ObjectId))
            .Select(i => i.ToTallyBallot(data.CiphertextTally)).ToList();
        Assert.That(result.Tally, Is.EqualTo(data.PlaintextTally));
        Assert.That(result.ChallengedBallots!.Count, Is.EqualTo(plaintextChallengedBallots.Count));
        Assert.That(result.ChallengedBallots, Is.EqualTo(plaintextChallengedBallots));
    }
}
