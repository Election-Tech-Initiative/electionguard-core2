
using ElectionGuard.Encryption.Utils.Generators;
using ElectionGuard.ElectionSetup.Tests.Generators;
using ElectionGuard.Decryption.Decryption;
using ElectionGuard.Decryption.Tally;
using ElectionGuard.Decryption.Tests.Tally;

namespace ElectionGuard.Decryption.Tests.Decryption;

// Simple tests using the programmatically generated fake manifest
[TestFixture]
public class TestDecryptWithSharesSimple : DisposableBase
{
    private const int BALLOT_COUNT_CAST = 2;
    private const int BALLOT_COUNT_SPOILED = 1;
    private const int NUMBER_OF_GUARDIANS = 3;
    private const int QUORUM = 2;

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
        Assert.That(plaintextTally.Tally, Is.EqualTo(data.PlaintextTally));
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

        using var mediator = new DecryptionMediator(
            "fake-mediator",
            data.CiphertextTally,
            guardians.Select(i => i.SharePublicKey()).ToList()
            );

        // Act
        foreach (var guardian in guardians)
        {
            using var share = guardian.ComputeDecryptionShare(data.CiphertextTally);
            mediator.SubmitShare(share!);
        }
        var plaintextTally = mediator.Decrypt(data.CiphertextTally.TallyId);

        // Assert
        Assert.That(plaintextTally.Tally, Is.EqualTo(data.PlaintextTally));
    }

    [Test]
    public void Test_Decrypt_Ballot_With_All_Guardians_Present_Simple()
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
                .ToList();
        var spoiledBallots = data.CiphertextBallots.Where(i => i.IsSpoiled).ToList();
        using var mediator = new DecryptionMediator(
            "fake-mediator",
            data.CiphertextTally,
            guardians.Select(i => i.SharePublicKey()).ToList());

        var plaintextSpoiledBallots = data.PlaintextBallots
            .Where(i => data.CiphertextTally.SpoiledBallotIds.Contains(i.ObjectId))
            .Select(i => i.ToTallyBallot(data.CiphertextTally)).ToList();

        // Act
        foreach (var guardian in guardians)
        {
            var shares = guardian.ComputeDecryptionShares(
                data.CiphertextTally, spoiledBallots);
            mediator.SubmitShares(shares, spoiledBallots);
        }

        var result = mediator.Decrypt(data.CiphertextTally.TallyId);

        // Assert
        Assert.That(result.Tally, Is.EqualTo(data.PlaintextTally));
        Assert.That(result.SpoiledBallots!.Count, Is.EqualTo(plaintextSpoiledBallots.Count));
        Assert.That(result.SpoiledBallots, Is.EqualTo(plaintextSpoiledBallots));
    }
}
