
using ElectionGuard.Encryption.Utils.Generators;
using ElectionGuard.ElectionSetup.Tests.Generators;
using ElectionGuard.Decryption.Decryption;
using ElectionGuard.Decryption.Tests.Tally;
using ElectionGuard.Decryption.Shares;
using ElectionGuard.Decryption.ChallengeResponse;

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

    [Test] // Category("SingleTest")
    public void Test_Decrypt_Tally_With_All_Guardians_Present_Simple()
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

        var guardians = data.KeyCeremony.Guardians.ToList();

        using var mediator = new DecryptionMediator(
            "fake-mediator",
            data.CiphertextTally,
            guardians.Select(i => i.SharePublicKey()).ToList());

        // Act
        foreach (var guardian in guardians)
        {
            // compute the share
            var share = guardian.ComputeDecryptionShare(data.CiphertextTally);
            mediator.SubmitShare(share!);
        }

        // create challenges
        var challenges = mediator.CreateChallenge(data.CiphertextTally.TallyId);
        foreach (var guardian in guardians)
        {
            var challenge = challenges[guardian.GuardianId];
            var share = mediator.GetShare(data.CiphertextTally.TallyId, guardian.GuardianId)!;

            // respond to challenge
            var response = guardian.ComputeChallengeResponse(share, challenge);
            mediator.SubmitResponse(data.CiphertextTally.TallyId, response!);
        }

        var validated = mediator.ValidateResponses(data.CiphertextTally.TallyId);
        Assert.That(validated, Is.True);

        var plaintextTally = mediator.Decrypt(data.CiphertextTally.TallyId);

        // Assert
        Assert.That(plaintextTally.Tally, Is.EqualTo(data.PlaintextTally));
    }

    [Test]
    public void Test_Decrypt_Tally_With_Quorum_Guardians_Present_Simple()
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
        using var data = TestDecryptionData.ConfigureTestCase(
            KeyCeremonyGenerator.GenerateKeyCeremonyData(
            NUMBER_OF_GUARDIANS,
            QUORUM, runKeyCeremony: true),
            ManifestGenerator.GetFakeManifest(),
            BALLOT_COUNT_CAST,
            BALLOT_COUNT_CHALLENGED,
            BALLOT_COUNT_SPOILED);

        var guardians = data.KeyCeremony.Guardians
            .ToList();
        var challengedBallots = data.CiphertextBallots
            .Where(i => i.IsChallenged).ToList();
        using var mediator = new DecryptionMediator(
            "fake-mediator",
            data.CiphertextTally,
            guardians.Select(i => i.SharePublicKey()).ToList());

        var plaintextChallengedBallots = data.PlaintextBallots
            .Where(i => data.CiphertextTally.ChallengedBallotIds.Contains(i.ObjectId))
            .Select(i => i.ToTallyBallot(data.CiphertextTally)).ToList();

        Console.WriteLine($"Challenged Ballots: {challengedBallots.Count}");

        // Act
        foreach (var guardian in guardians)
        {
            var shares = guardian.ComputeDecryptionShares(
                data.CiphertextTally, challengedBallots);
            mediator.SubmitShares(shares, challengedBallots);
        }

        var result = mediator.Decrypt(data.CiphertextTally.TallyId);

        // Assert
        Assert.That(result.Tally, Is.EqualTo(data.PlaintextTally));
        Assert.That(result.ChallengedBallots!.Count, Is.EqualTo(plaintextChallengedBallots.Count));
        Assert.That(result.ChallengedBallots, Is.EqualTo(plaintextChallengedBallots));
    }
}
