
using ElectionGuard.Encryption.Utils.Generators;
using ElectionGuard.ElectionSetup.Tests.Generators;
using ElectionGuard.Decryption.Decryption;
using ElectionGuard.Decryption.Shares;
using ElectionGuard.Decryption.ChallengeResponse;
using ElectionGuard.Decryption.Extensions;
using ElectionGuard.Decryption.Accumulation;
using ElectionGuard.Decryption.Challenge;
using ElectionGuard.Decryption.Tests.Decryption;

namespace ElectionGuard.Decryption.Tests.Challenge;

// Simple tests using the programmatically generated fake manifest
[TestFixture]
public class TestChallenge : DisposableBase
{
    private const int BALLOT_COUNT_CAST = 2;
    private const int BALLOT_COUNT_CHALLENGED = 1;
    private const int BALLOT_COUNT_SPOILED = 1;
    private const int NUMBER_OF_GUARDIANS = 3;
    private const int QUORUM = 2;

    [Test, Category("SingleTest")]
    public void Test_Challenge_Selection()
    {
        var seed = new ElementModQ(Constants.TWO_MOD_Q);
        var nonce = new ElementModQ(Constants.TWO_MOD_Q);

        // Arrange
        using var data = TestDecryptionData.ConfigureTestCase(
            KeyCeremonyGenerator.GenerateKeyCeremonyData(
            NUMBER_OF_GUARDIANS,
            QUORUM, runKeyCeremony: true),
            ManifestGenerator.GetFakeManifest(startDateTime: DateTime.UtcNow.Date),
            BALLOT_COUNT_CAST,
            BALLOT_COUNT_CHALLENGED,
            BALLOT_COUNT_SPOILED,
            seed, nonce);

        var context = data.CiphertextTally.Context;

        var guardians = data.KeyCeremony.Guardians.ToList();
        var coefficients = guardians.ComputeLagrangeCoefficients();
        var contest = data.CiphertextTally.Contests.Values.First();
        var selection = contest.Selections.Values.First();
        var plaintext = data.PlaintextTally.Contests.Values
            .First(i => i.ObjectId == contest.ObjectId).Selections.Values
            .First(i => i.ObjectId == selection.ObjectId);

        // Act
        var shares = new Dictionary<string, SelectionShare>();
        foreach (var guardian in guardians)
        {
            // compute the share
            var share = guardian.ComputeDecryptionShare(selection);
            shares.Add(guardian.GuardianId, share!);
        }

        // amalgamate shares
        var guardianShares = shares.Select(i =>
        {
            var guardian = guardians.First(g => g.GuardianId == i.Key);
            return Tuple.Create(guardian.SharePublicKey(), i.Value);
        }).ToList();

        // accumulate shares
        var accumulation = selection.AccumulateShares(
            guardianShares, coefficients);

        // create challenge
        var challenge = SelectionChallenge.ComputeChallenge(
                    context,
                    selection,
                    accumulation);
        Console.WriteLine($"Challenge: {challenge}");

        // interpolate challenge coefficients
        var challenges = new Dictionary<string, SelectionChallenge>();
        foreach (var (guardianId, coefficient) in coefficients)
        {
            var guardian = guardians.First(g => g.GuardianId == guardianId);

            // ci =(c·wi)modq.
            var guardianChallenge = new SelectionChallenge(
                selection, guardian, coefficient, challenge);

            Console.WriteLine($"Guardian Challenge: {guardianChallenge.Challenge}");

            challenges.Add(
                guardianId,
                guardianChallenge
                );
        }

        // respond to challenge
        var responses = new Dictionary<string, SelectionChallengeResponse>();
        foreach (var (guardianId, guardianChallenge) in challenges)
        {
            var guardian = guardians.First(g => g.GuardianId == guardianId);
            var share = shares[guardianId];

            // vi = (ui − ciP(i)) mod q.
            var response = guardian.ComputeChallengeResponse(share, guardianChallenge);
            responses.Add(guardianId, response!);
        }

        // validate responses
        foreach (var (guardianId, response) in responses)
        {
            var guardian = guardians.First(g => g.GuardianId == guardianId);
            var share = shares[guardianId];
            var guardianChallenge = challenges[guardianId];

            var validated = response.IsValid(
                selection, guardian.SharePublicKey(), share, guardianChallenge);

            Assert.That(validated, Is.True);
        }

        // create the proof
        var proof = accumulation.ComputeProof(
            challenge, responses.Values.ToList());
        accumulation.AddProof(proof);

        // decrypt
        var decrypted = selection.Decrypt(accumulation);

        // TODO: verify the proof

        Assert.That(decrypted.Tally, Is.EqualTo(plaintext.Tally));

    }
}
