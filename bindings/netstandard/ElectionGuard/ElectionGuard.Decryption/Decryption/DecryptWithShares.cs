
using ElectionGuard.Ballot;
using ElectionGuard.Decryption.Accumulation;
using ElectionGuard.Decryption.Extensions;
using ElectionGuard.Decryption.Shares;
using ElectionGuard.Decryption.Tally;
using ElectionGuard.Guardians;

namespace ElectionGuard.Decryption.Decryption;

public static class DecryptWithSharesExtensions
{
    public static PlaintextTally Decrypt(
        this CiphertextTally self,
        List<Tuple<ElectionPublicKey, TallyShare>> guardianShares,
        bool skipValidation = false)
    {
        var lagrangeCoefficients = guardianShares.ComputeLagrangeCoefficients();
        return self.Decrypt(
            guardianShares, lagrangeCoefficients, skipValidation
        );
    }

    public static PlaintextTally Decrypt(
        this CiphertextTally self,
        List<Tuple<ElectionPublicKey, TallyShare>> guardianShares,
        Dictionary<string, LagrangeCoefficient> lagrangeCoefficients,
        bool skipValidation = false)
    {
        var accumulation = self.AccumulateShares(
            guardianShares, lagrangeCoefficients, skipValidation);

        return self.Decrypt(accumulation.Contests.Values.ToList());
    }

    public static PlaintextTally Decrypt(
        this CiphertextTally self,
        List<AccumulatedContest> decryptions)
    {
        // Create a plaintext tally from the ciphertext tally
        var plaintextTally = new PlaintextTally(
            self.TallyId, self.Name, self.Manifest);

        // iterate over the ciphertext contests
        foreach (var contest in self.Contests)
        {
            // get the contest from the plaintext tally
            var plaintextContest = plaintextTally.Contests.First(
                x => x.Key == contest.Key).Value;

            var contestAccumulation = decryptions.First(
                x => x.ObjectId == contest.Key);

            foreach (var selection in contest.Value.Selections)
            {
                // get the selection from the plaintext contest
                var plaintextSelection = plaintextContest.Selections.First(
                    x => x.Key == selection.Key).Value;

                var decryption = contestAccumulation.Selections.First(
                    x => x.Key == selection.Key).Value;

                // decrypt the selection
                var ciphertext = selection.Value;
                var value = ciphertext!.Decrypt(decryption);

                // add the decrypted value to the plaintext selection
                plaintextSelection.Tally += value.Tally;
            }
        }

        return plaintextTally;
    }

    /// <summary>
    /// Use the Ciphertext Tally to decrypt the provided ballot shares.
    /// </summary>
    /// <param name="self">The Ciphertext Tally</param>
    /// <param name="ballotShares">The ballot shares as a dictionary with the ballot id as the key</param>
    public static List<PlaintextTallyBallot> Decrypt(
        this Dictionary<string, CiphertextBallot> self,
        Dictionary<string, CiphertextDecryptionBallot> ballotShares,
        Dictionary<string, LagrangeCoefficient> lagrangeCoefficients,
        CiphertextTally tally,
        bool skipValidation = false)
    {
        var plaintextBallots = new List<PlaintextTallyBallot>();
        foreach (var (ballotId, ballot) in self)
        {
            var plaintextBallot = ballot.Decrypt(
                ballotShares[ballotId], lagrangeCoefficients,
                tally,
                skipValidation);
            plaintextBallots.Add(plaintextBallot);
        }

        return plaintextBallots;
    }

    /// <summary>
    /// Decrypt a single ballot using the provided ballot shares
    /// </summary>
    public static PlaintextTallyBallot Decrypt(
        this CiphertextBallot self,
        CiphertextDecryptionBallot ballotShares,
        Dictionary<string, LagrangeCoefficient> lagrangeCoefficients,
        CiphertextTally tally,
        bool skipValidation = false)
    {
        return self.Decrypt(
            ballotShares.GetShares(),
            lagrangeCoefficients,
            tally.TallyId,
            tally.Context.CryptoExtendedBaseHash,
            skipValidation);
    }

    /// <summary>
    /// Decrypt a single ballot using the provided ballot shares
    /// </summary>
    public static PlaintextTallyBallot Decrypt(
        this CiphertextBallot self,
        CiphertextDecryptionBallot ballotShares,
        Dictionary<string, LagrangeCoefficient> lagrangeCoefficients,
        string tallyId,
        ElementModQ extendedBaseHash,
        bool skipValidation = false)
    {
        return self.Decrypt(
            ballotShares.GetShares(),
            lagrangeCoefficients,
            tallyId,
            extendedBaseHash,
            skipValidation);
    }

    /// <summary>
    /// Decrypt a single ballot using the provided ballot shares
    /// </summary>
    public static PlaintextTallyBallot Decrypt(
        this CiphertextBallot self,
        List<Tuple<ElectionPublicKey, BallotShare>> guardianShares,
        string tallyId,
        ElementModQ extendedBaseHash,
        bool skipValidation = false)
    {
        var lagrangeCoefficients = guardianShares.ComputeLagrangeCoefficients();
        return self.Decrypt(
            guardianShares, lagrangeCoefficients, tallyId, extendedBaseHash, skipValidation
        );
    }

    /// <summary>
    /// Decrypt a single ballot using the provided ballot shares
    /// </summary>
    public static PlaintextTallyBallot Decrypt(
        this CiphertextBallot self,
        List<Tuple<ElectionPublicKey, BallotShare>> guardianShares,
        Dictionary<string, LagrangeCoefficient> lagrangeCoefficients,
        string tallyId,
        ElementModQ extendedBaseHash,
        bool skipValidation = false)
    {
        // Accumulate the shares
        var accumulation = self.AccumulateShares(
            guardianShares,
            lagrangeCoefficients,
             extendedBaseHash,
             skipValidation);

        return self.Decrypt(
            accumulation.Contests.Values.ToList(), tallyId);
    }

    public static PlaintextTallyBallot Decrypt(
        this CiphertextBallot self,
        List<AccumulatedContest> decryptions, string tallyId)
    {
        // create a plaintext tally from the first ballot share's style Id.
        var plaintextTally = new PlaintextTallyBallot(tallyId, self);

        // iterate over the contests from the ballot.
        foreach (var contest in self.Contests)
        {
            var plaintextContest = plaintextTally.Contests.First(
                x => x.Key == contest.ObjectId).Value;

            var contestAccumulation = decryptions.First(
                x => x.ObjectId == contest.ObjectId);

            // iterate over the selections from the contest
            foreach (var selection in contest.Selections.Where(x => x.IsPlaceholder == false))
            {
                var plaintextSelection = plaintextContest.Selections.First(
                    x => x.Key == selection.ObjectId).Value;

                var decryption = contestAccumulation.Selections.First(
                    x => x.Key == selection.ObjectId).Value;

                var value = selection.Decrypt(decryption);
                plaintextSelection.Tally += value.Tally;
            }
        }

        return plaintextTally;
    }

    /// <summary>
    /// Decrypt a single selection using the provided selection shares
    /// </summary>
    public static PlaintextTallySelection Decrypt(
        this CiphertextTallySelection self,
        List<Tuple<ElectionPublicKey, SelectionShare>> guardianShares,
        ElementModQ extendedBaseHash,
        bool skipValidation = false)
    {
        var lagrangeCoefficients = guardianShares
            .Select(x => x.Item1)
            .ToList()
            .ComputeLagrangeCoefficients();
        var decryption = self.AccumulateShares(
            guardianShares, lagrangeCoefficients, extendedBaseHash, skipValidation);
        return self.Decrypt(decryption);
    }

    /// <summary>
    /// Decrypt a single selection using the provided selection shares
    /// </summary>
    public static PlaintextTallySelection Decrypt(
        this ICiphertextSelection self,
        List<Tuple<ElectionPublicKey, SelectionShare>> guardianShares,
        Dictionary<string, LagrangeCoefficient> lagrangeCoefficients,
        ElementModQ extendedBaseHash, bool skipValidation = false)
    {
        // accumulate all of the shares calculated for the selection
        var decryption = self.AccumulateShares(
            guardianShares, lagrangeCoefficients, extendedBaseHash, skipValidation);
        return self.Decrypt(decryption);
    }

    /// <summary>
    /// Decrypt a single selection using the provided accumulated decryption.
    /// </summary>
    public static PlaintextTallySelection Decrypt(
        this ICiphertextSelection self,
        AccumulatedSelection decryption)
    {
        // Calculate 𝑀=𝐵⁄(∏𝑀𝑖) mod 𝑝.
        var tally = self.Ciphertext.Decrypt(decryption.Value);
        if (!tally.HasValue)
        {
            throw new Exception("Failed to decrypt selection");
        }

        var plaintext = new PlaintextTallySelection(
            self, tally ?? 0UL, decryption.Value!, decryption.Proof!);
        return plaintext;
    }
}
