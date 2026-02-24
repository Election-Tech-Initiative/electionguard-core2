using ElectionGuard.Ballot;
using ElectionGuard.Decryption.Decryption;
using ElectionGuard.Decryption.Shares;
using ElectionGuard.Decryption.Tally;
using ElectionGuard.Guardians;

namespace ElectionGuard.Decryption.Accumulation;

public static class AccumulateSharesExtensions
{
    public static AccumulatedTally AccumulateShares(
        this CiphertextTally self,
        List<Tuple<ElectionPublicKey, TallyShare>> guardianShares,
        Dictionary<string, LagrangeCoefficient> lagrangeCoefficients,
        bool skipValidation = false)
    {
        // TODO: move this up the stack
        if (!skipValidation)
        {
            if (guardianShares.Count < (int)self.Context.Quorum)
            {
                throw new Exception("Not enough guardian shares provided");
            }

            if (guardianShares.Count != lagrangeCoefficients.Count)
            {
                throw new Exception("Mismatched lagrange coefficients provided");
            }

            // check that all the shares are valid
            foreach (var (guardian, share) in guardianShares)
            {
                if (!share.IsValid(self, guardian))
                {
                    throw new Exception("Tally share is not valid");
                }
            }
        }

        var contests = new List<AccumulatedContest>();

        // accumulate all of the shares calculated for the tally
        foreach (var (contestId, contest) in self.Contests)
        {
            var accumulator = new AccumulatedContest(
                contest,
                contest.Selections
                    .Select(i => (IElectionSelection)i.Value)
                    .ToList());
            var contestShares = guardianShares
                .Select(
                    x => x.Item2.GetContestShare(x.Item1, contest.ObjectId))
                .ToList();

            accumulator.Accumulate(
                contestShares,
                lagrangeCoefficients,
                skipValidation);

            contests.Add(accumulator);
        }

        return new AccumulatedTally(self.TallyId, contests);
    }

    public static List<AccumulatedBallot> AccumulateShares(
        this Dictionary<string, CiphertextBallot> self,
        string tallyId,
        Dictionary<string, CiphertextDecryptionBallot> ballotShares,
        Dictionary<string, LagrangeCoefficient> lagrangeCoefficients,
        ElementModQ extendedBaseHash,
        bool skipValidation = false)
    {
        var ballots = new List<AccumulatedBallot>();
        foreach (var (ballotId, ballot) in self)
        {
            var accumulation = ballot.AccumulateShares(
                tallyId,
                ballotShares[ballotId],
                lagrangeCoefficients,
                extendedBaseHash,
                skipValidation);
            ballots.Add(accumulation);
        }
        return ballots;
    }

    public static AccumulatedBallot AccumulateShares(
        this CiphertextBallot self,
        string tallyId,
        CiphertextDecryptionBallot ballotShares,
        Dictionary<string, LagrangeCoefficient> lagrangeCoefficients,
        ElementModQ extendedBaseHash,
        bool skipValidation = false)
    {
        return self.AccumulateShares(
            tallyId,
            ballotShares.GetShares(),
            lagrangeCoefficients,
            extendedBaseHash,
            skipValidation);
    }

    /// <summary>
    /// Decrypt a single ballot using the provided ballot shares
    /// </summary>
    public static AccumulatedBallot AccumulateShares(
        this CiphertextBallot self,
        string tallyId,
        List<Tuple<ElectionPublicKey, BallotShare>> guardianShares,
        Dictionary<string, LagrangeCoefficient> lagrangeCoefficients,
        ElementModQ extendedBaseHash,
        bool skipValidation = false)
    {
        // TODO: maybe move this out to a higher level?
        if (!skipValidation)
        {
            if (guardianShares.Count != lagrangeCoefficients.Count)
            {
                throw new Exception("Mismatched lagrange coefficients provided");
            }

            // check that all the shares are valid
            // note this version does not check the validity of the contests
            // since we do not have the ciphertext of the ballot
            foreach (var (guardian, share) in guardianShares)
            {
                if (!share.IsValid(self, guardian, extendedBaseHash))
                {
                    throw new Exception("Ballot share is not valid");
                }
            }
        }

        var contests = new List<AccumulatedContest>();

        // accumulate all of the shares calculated for the ballot
        foreach (var contest in self.Contests)
        {
            var accumulator = new AccumulatedContest(
                contest, contest.Selections.Select(i => (IElectionSelection)i).ToList());
            var contestShares = guardianShares.Select(
                x => x.Item2.GetContestShare(x.Item1, contest.ObjectId))
            .ToList();

            accumulator.Accumulate(
                contestShares,
                lagrangeCoefficients,
                skipValidation);

            contests.Add(accumulator);
        }

        return new AccumulatedBallot(tallyId, self.ObjectId, contests);
    }

    /// <summary>
    /// Accumulate the guardian shares for a selection.
    /// Computes 𝑀𝑏𝑎𝑟 = 𝑀𝑏𝑎𝑟 * (𝑀𝑖 ^ 𝑤𝑖) mod p
    /// </summary>
    public static AccumulatedSelection AccumulateShares(
        this ICiphertextSelection self,
        List<Tuple<ElectionPublicKey, SelectionShare>> guardianShares,
        Dictionary<string, LagrangeCoefficient> lagrangeCoefficients,
        ElementModQ extendedBaseHash,
        bool skipValidation = false)
    {
        if (!skipValidation)
        {
            foreach (var (guardian, share) in guardianShares)
            {
                if (!share.IsValid(self, guardian))
                {
                    throw new Exception(
                        $"Failed to verify selection share for guardian {guardian.GuardianId}");
                }
            }
        }

        return self.AccumulateShares(guardianShares, lagrangeCoefficients, skipValidation);
    }

    /// <summary>
    /// Accumulate the guardian shares for a selection.
    /// Computes 𝑀𝑏𝑎𝑟 = Π (𝑀𝑖 ^ 𝑤𝑖) mod p
    /// </summary>
    public static AccumulatedSelection AccumulateShares(
        this ICiphertextSelection self,
        List<Tuple<ElectionPublicKey, SelectionShare>> guardianShares,
        Dictionary<string, LagrangeCoefficient> lagrangeCoefficients,
        bool skipValidation = false)
    {
        // accumulate all of the shares calculated for the selection
        var decryption = new AccumulatedSelection(self);

        // 𝑀𝑏𝑎𝑟 = Π (𝑀𝑖 ^ 𝑤𝑖) mod p
        decryption.Accumulate(guardianShares, lagrangeCoefficients, skipValidation);
        return decryption;
    }
}
