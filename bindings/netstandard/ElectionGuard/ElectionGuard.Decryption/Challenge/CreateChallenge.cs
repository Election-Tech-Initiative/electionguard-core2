


using ElectionGuard.Decryption.Accumulation;
using ElectionGuard.Decryption.Extensions;
using ElectionGuard.Decryption.Tally;
using ElectionGuard.Guardians;

namespace ElectionGuard.Decryption.Challenge;
// TODO: async versions
public static class CreateChallengeExtensions
{
    /// <summary>
    /// Create a challenge for the tally. Accepts a ciphertext tally which contains 
    /// the accumulation of all selections on all of all ballots cast and the Accumulated tally
    /// with is the accumulation of all decryption shares submitted by guardains for each selection
    /// in the ciphertext tally.
    ///
    /// A challenge is created for each selection and then offset by the lagrange coefficient
    /// to craete a guardian-scoped challenge for each selection.
    /// </summary>
    public static Dictionary<string, TallyChallenge> CreateChallenge(
        this CiphertextTally self, AccumulatedTally accumulatedTally,
        Dictionary<string, ElectionPublicKey> _guardians)
    {
        var coefficients = _guardians.Values.ToList().ComputeLagrangeCoefficients();
        return self.CreateChallenge(accumulatedTally, coefficients);
    }

    /// <summary>
    /// Create a challenge for the tally. Accepts a ciphertext tally which contains 
    /// the accumulation of all selections on all of all ballots cast and the Accumulated tally
    /// with is the accumulation of all decryption shares submitted by guardains for each selection
    /// in the ciphertext tally.
    ///
    /// A challenge is created for each selection and then offset by the lagrange coefficient
    /// to craete a guardian-scoped challenge for each selection.
    ///
    /// Thiss override accepts existing lagrange coefficients to be used in the challenge
    /// </summary>
    public static Dictionary<string, TallyChallenge> CreateChallenge(
        this CiphertextTally self, AccumulatedTally accumulatedTally,
        Dictionary<string, LagrangeCoefficient> lagrangeCoefficients)
    {
        var challenges = new Dictionary<string, TallyChallenge>();

        foreach (var (guardianId, coefficient) in lagrangeCoefficients)
        {
            challenges.Add(
                guardianId,
                new TallyChallenge(guardianId, self.TallyId, coefficient, self.Manifest));
        }

        return self.CreateChallenge(accumulatedTally, challenges);
    }

    /// <summary>
    /// Create a challenge for the tally. Accepts a ciphertext tally which contains 
    /// the accumulation of all selections on all of all ballots cast and the Accumulated tally
    /// with is the accumulation of all decryption shares submitted by guardains for each selection
    /// in the ciphertext tally.
    ///
    /// A challenge is created for each selection and then offset by the lagrange coefficient
    /// to craete a guardian-scoped challenge for each selection.
    ///
    /// This override accepts a collection of existing challenge objects that will be filled in and returned to the caller.
    /// </summary>
    public static Dictionary<string, TallyChallenge> CreateChallenge(
        this CiphertextTally self, AccumulatedTally accumulatedTally,
        Dictionary<string, TallyChallenge> challenges)
    {
        foreach (var (contestId, contest) in self.Contests)
        {
            if (!accumulatedTally.Contests.ContainsKey(contestId))
            {
                throw new KeyNotFoundException($"Contest {contestId} not found in accumulated tally");
            }

            var contestAccumulation = accumulatedTally.Contests[contestId];

            foreach (var (selectionId, selection) in contest.Selections)
            {
                if (!contestAccumulation.Selections.ContainsKey(selectionId))
                {
                    throw new KeyNotFoundException($"Selection {selectionId} not found in accumulated tally");
                }

                // get the accumlation for this selection
                var selectionAccumulation = contestAccumulation.Selections[selectionId];

                using var challenge = SelectionChallenge.ComputeChallenge(
                    self.Context,
                    selection,
                    selectionAccumulation);

                // iterate over the challenges and add the selections
                foreach (var (_, tallyChallenge) in challenges)
                {
                    tallyChallenge.Add(contest, selection, challenge);
                }
            }
        }

        return challenges;
    }

    /// <summary>
    /// Create a challenge for a collection of ballots. 
    /// Accepts a collection of ciphertext ballots and
    /// a collection of accumulated ballots which contains  
    /// the accumulation of all decryption shares submitted by guardains for each selection
    /// in each ballot.
    ///
    /// A challenge is created for each selection and then offset by the lagrange coefficient
    /// to craete a guardian-scoped challenge for each selection.
    /// </summary>
    public static Dictionary<string, List<BallotChallenge>> CreateChallenges(
        this List<CiphertextBallot> self, List<AccumulatedBallot> accumulatedBallot,
        string tallyId,
        Dictionary<string, LagrangeCoefficient> lagrangeCoefficients,
        CiphertextElectionContext context)
    {
        var challenges = new Dictionary<string, List<BallotChallenge>>();

        foreach (var ballot in self)
        {
            // validate the ballot has an accumulated version
            if (!accumulatedBallot.Any(x => x.ObjectId == ballot.ObjectId))
            {
                throw new KeyNotFoundException($"Ballot {ballot.ObjectId} not found in accumulated ballots");
            }

            var ballotAccumulation = accumulatedBallot.First(x => x.ObjectId == ballot.ObjectId);
            var ballotChallenges = ballot.CreateChallenge(
                ballotAccumulation, tallyId, lagrangeCoefficients, context);

            // iterate over the challenges and add tto the result
            foreach (var (guardianId, challenge) in ballotChallenges)
            {
                if (!challenges.ContainsKey(guardianId))
                {
                    challenges.Add(guardianId, new List<BallotChallenge>());
                }
                challenges[guardianId].Add(challenge);
            }
        }
        return challenges;
    }

    public static Dictionary<string, BallotChallenge> CreateChallenge(
        this CiphertextBallot self, AccumulatedBallot accumulatedBallot,
        string tallyId,
        Dictionary<string, ElectionPublicKey> _guardians,
        CiphertextElectionContext context)
    {
        var coefficients = _guardians.Values.ToList().ComputeLagrangeCoefficients();
        return self.CreateChallenge(
            accumulatedBallot, tallyId, coefficients, context.CryptoExtendedBaseHash, context.ElGamalPublicKey);
    }

    public static Dictionary<string, BallotChallenge> CreateChallenge(
        this CiphertextBallot self, AccumulatedBallot accumulatedBallot,
        string tallyId,
        Dictionary<string, ElectionPublicKey> _guardians,
        ElementModQ extendedBaseHash, ElementModP elGamalPublicKey)
    {
        var coefficients = _guardians.Values.ToList().ComputeLagrangeCoefficients();
        return self.CreateChallenge(
            accumulatedBallot, tallyId, coefficients, extendedBaseHash, elGamalPublicKey);
    }

    public static Dictionary<string, BallotChallenge> CreateChallenge(
        this CiphertextBallot self, AccumulatedBallot accumulatedBallot,
        string tallyId,
        Dictionary<string, LagrangeCoefficient> coefficients,
        CiphertextElectionContext context)
    {
        return self.CreateChallenge(
            accumulatedBallot, tallyId, coefficients, context.CryptoExtendedBaseHash, context.ElGamalPublicKey);
    }

    public static Dictionary<string, BallotChallenge> CreateChallenge(
        this CiphertextBallot self, AccumulatedBallot accumulatedBallot,
        string tallyId,
        Dictionary<string, LagrangeCoefficient> lagrangeCoefficients,
        ElementModQ extendedBaseHash, ElementModP elGamalPublicKey)
    {
        var challenges = new Dictionary<string, BallotChallenge>();

        foreach (var (guardianId, coefficient) in lagrangeCoefficients)
        {
            challenges.Add(
                guardianId,
                new BallotChallenge(tallyId, guardianId, coefficient, self));
        }
        return self.CreateChallenge(
            accumulatedBallot, challenges, extendedBaseHash, elGamalPublicKey);
    }

    public static Dictionary<string, BallotChallenge> CreateChallenge(
        this CiphertextBallot self, AccumulatedBallot accumulatedBallot,
        Dictionary<string, BallotChallenge> challenges,
        ElementModQ extendedBaseHash, ElementModP elGamalPublicKey)
    {
        foreach (var contest in self.Contests)
        {
            if (!accumulatedBallot.Contests.ContainsKey(contest.ObjectId))
            {
                throw new KeyNotFoundException($"Contest {contest.ObjectId} not found in accumulated tally");
            }

            var contestAccumulation = accumulatedBallot.Contests[contest.ObjectId];

            foreach (var selection in contest.Selections)
            {
                if (!contestAccumulation.Selections.ContainsKey(selection.ObjectId))
                {
                    throw new KeyNotFoundException($"Selection {selection.ObjectId} not found in accumulated tally");
                }

                // get the accumlation for this selection
                var selectionAccumulation = contestAccumulation.Selections[selection.ObjectId];

                using var challenge = SelectionChallenge.ComputeChallenge(
                    extendedBaseHash,
                    elGamalPublicKey,
                    selection,
                    selectionAccumulation);

                // iterate over the challenges and add the selections
                foreach (var (_, ballotChallenge) in challenges)
                {
                    ballotChallenge.Add(contest, selection, challenge);
                }
            }
        }

        return challenges;
    }
}

