
using ElectionGuard.Encryption.Utils.Generators;
using ElectionGuard.ElectionSetup.Tests.Generators;
using ElectionGuard.Decryption.Decryption;
using ElectionGuard.Decryption.Tally;

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

        var mediator = new DecryptionMediator(
            "fake-mediator",
            data.CiphertextTally,
            guardians.Select(i => i.SharePublicKey()).ToList());

        // Act
        var spoiledBallots = data.CiphertextBallots.Where(i => i.IsSpoiled).ToList();
        var nonce = spoiledBallots.First().Nonce;
        foreach (var guardian in guardians)
        {
            var shares = guardian.ComputeDecryptionShares(
                data.CiphertextTally, spoiledBallots);
            mediator.SubmitShares(shares);
        }
        var result = mediator.Decrypt(data.CiphertextTally.TallyId);

        var plaintextBallots = data.PlaintextBallots
            .Where(i => data.CiphertextTally.SpoiledBallotIds.Contains(i.ObjectId))
            .Select(i => ToTallyBallot(data.CiphertextTally, i)).ToList();

        // Assert
        Assert.That(result.Tally, Is.EqualTo(data.PlaintextTally));
        Assert.That(result.SpoiledBallots, Is.EqualTo(plaintextBallots));
    }

    private static PlaintextTallyBallot ToTallyBallot(CiphertextTally tally, PlaintextBallot ballot)
    {
        var plaintext = new PlaintextTallyBallot(tally.TallyId, ballot.ObjectId, ballot.StyleId, tally.Manifest);
        foreach (var (contestId, contest) in plaintext.Contests)
        {
            var plaintextContest = ballot.Contests.First(i => i.ObjectId == contestId);
            foreach (var (selectionId, selection) in contest.Selections)
            {
                var plaintextSelection = plaintextContest.Selections.First(i => i.ObjectId == selectionId);
                selection.Tally = plaintextSelection.Vote;
                // TODO: add support for extended data
            }
        }

        return plaintext;
    }
}
