
using ElectionGuard.Ballot;
using ElectionGuard.Encryption.Utils.Generators;
using ElectionGuard.Decryption.Decryption;
using ElectionGuard.Decryption.Tests.Tally;
using ElectionGuard.ElectionSetup.Tests.Generators;
using ElectionGuard.Decryption.ChallengeResponse;
using ElectionGuard.Decryption.Shares;
using ElectionGuard.Decryption.Extensions;

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

    [Test, Category("OneShot")]
    public void Test_Decrypt_Ballot_With_All_Guardians_Present_Simple()
    {
        var tallyId = "fake-tally";
        using var seed = Constants.ONE_MOD_Q;
        using var nonce = Constants.ONE_MOD_Q;
        using var keyCeremony = KeyCeremonyGenerator.GenerateKeyCeremonyData(
            NUMBER_OF_GUARDIANS,
            QUORUM, runKeyCeremony: true);
        using var manifest = ManifestGenerator.GetFakeManifest();
        using var data = ElectionGenerator.GenerateFakeElectionData(
            NUMBER_OF_GUARDIANS, QUORUM, manifest);

        using var ballot = BallotGenerator.GetFakeBallot(data.InternalManifest);

        // Act
        using var ciphertext = Encrypt.Ballot(
            ballot, data.InternalManifest, data.Context, seed, nonce);
        using var ballotNonce = new ElementModQ(ciphertext.Nonce);
        ciphertext.Challenge();

        var guardians = keyCeremony.Guardians
            .ToList();

        // compute lagrange ciefficients
        var lagrangeCoefficients = guardians
            .Select(i => i.SharePublicKey()).ToList()
            .ComputeLagrangeCoefficients();

        var shares = new Dictionary<string, CiphertextDecryptionBallot>(){
            {
                ballot.ObjectId,
                new CiphertextDecryptionBallot(
                    ballot.ObjectId, ballot.StyleId, data.InternalManifest.ManifestHash)
            }
        };

        // Create Shares
        foreach (var guardian in guardians)
        {
            var share = guardian.ComputeDecryptionShare(
                tallyId, ciphertext);
            shares[share.BallotId].AddShare(
                share, guardian.SharePublicKey());
        }

        var result = ciphertext.IsValid(data.InternalManifest);
        Assert.That(result.IsValid, Is.True);

        // Decrypt with Nonce
        using var nonceDecrypted = ciphertext.Decrypt(
            data.InternalManifest, data.Context, ballotNonce);

        Console.WriteLine($"nonceDecrypted: {nonceDecrypted.ToJson()}");

        // Decrypt with Shares
        using var shareDecrypted = ciphertext.Decrypt(
            shares[ballot.ObjectId],
            lagrangeCoefficients,
            tallyId,
            data.Context.ElGamalPublicKey,
            data.Context.CryptoExtendedBaseHash
            );

        Console.WriteLine($"shareDecrypted: {shareDecrypted}");

        // TODO: assert share and secret rtunr same resutl

        //Assert.That(nonceDecrypted, Is.EqualTo(ballot));
    }

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
