
using ElectionGuard.Encryption.Utils.Generators;
using ElectionGuard.Decryption.Decryption;
using ElectionGuard.Decryption.Shares;
using ElectionGuard.Decryption.ChallengeResponse;
using ElectionGuard.Decryption.Extensions;
using ElectionGuard.Decryption.Accumulation;
using ElectionGuard.Decryption.Challenge;
using ElectionGuard.Decryption.Tests.Decryption;
using ElectionGuard.ElectionSetup.Tests.Generators;

namespace ElectionGuard.Decryption.Tests.Challenge;

// Simple tests using the programmatically generated fake manifest
[TestFixture]
public class TestChallenge : DisposableBase
{
    private const int BALLOT_COUNT_CAST = 1;
    private const int BALLOT_COUNT_CHALLENGED = 0;
    private const int BALLOT_COUNT_SPOILED = 0;
    private const int NUMBER_OF_GUARDIANS = 3;
    private const int QUORUM = 2;

    [Test] // Category("SingleTest")
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

        // Console.WriteLine($"context   : {context}");
        // Console.WriteLine($"Plaintext : {plaintext}");
        // Console.WriteLine($"Ciphertext: {selection}");

        // Act
        var shares = new Dictionary<string, SelectionShare>();
        foreach (var guardian in guardians)
        {
            // compute the share & the commitment that goes with it
            // 𝑀𝑖 = 𝐴^𝑠𝑖 mod 𝑝                              Equation (55)
            // (𝑎𝑖, 𝑏𝑖) = (𝑔^𝑢𝑖 mod 𝑝, 𝐴^𝑢𝑖 mod 𝑝)          Equation (58)
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
        // 𝑀𝑏𝑎𝑟 = Π (𝑀𝑖 ^ 𝑤𝑖) mod p Equation (57)
        var accumulation = selection.AccumulateShares(
            guardianShares, coefficients);

        // create challenge
        // c = H(06,Q;K,A,B,a,b,M) Equation (60)
        var challenge = SelectionChallenge.ComputeChallenge(
                    context,
                    selection,
                    accumulation);

        // interpolate challenge coefficients
        var challenges = new Dictionary<string, SelectionChallenge>();
        foreach (var (guardianId, coefficient) in coefficients)
        {
            var guardian = guardians.First(g => g.GuardianId == guardianId);

            // ci =(c·wi)modq. Equation (61)
            var guardianChallenge = new SelectionChallenge(
                selection, guardian, coefficient, challenge);

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

            // vi = (ui − 𝑐𝑖P(i)) mod q. Equation (62)
            var response = guardian.ComputeChallengeResponse(share, guardianChallenge);
            responses.Add(guardianId, response!);
        }

        var guardianKeys = guardians.Select(i => i.SharePublicKey())
                .ToDictionary(i => i.GuardianId);

        // validate responses
        foreach (var (guardianId, response) in responses)
        {
            var guardian = guardians.First(g => g.GuardianId == guardianId);
            var share = shares[guardianId];
            var guardianChallenge = challenges[guardianId];

            // 𝑎𝑖 = 𝑔^𝑣𝑖 • 𝐾^𝑐𝑖 mod 𝑝 Equation (63)
            // 𝑏𝑖 = 𝐴^𝑣𝑖 • 𝑀𝑖^𝑐𝑖 mod 𝑝 Equation (64)
            var validated = response.IsValid(
                selection,
                guardian.CommitmentOffset!,
                share, guardianChallenge);

            Assert.That(validated, Is.True);
        }

        // create the proof
        // Equation (65)
        var proof = accumulation.ComputeProof(
            challenge, responses.Values.ToList());
        accumulation.AddProof(proof);

        // decrypt
        var decrypted = selection.Decrypt(accumulation);

        // TODO: verify the proof

        Assert.That(decrypted.Tally, Is.EqualTo(plaintext.Tally));
    }
}
