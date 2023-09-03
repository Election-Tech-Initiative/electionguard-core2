
using ElectionGuard.Ballot;
using ElectionGuard.ElectionSetup;
using ElectionGuard.Decryption.Accumulation;
using ElectionGuard.Decryption.Extensions;
using ElectionGuard.Decryption.Shares;
using ElectionGuard.Decryption.Tally;
using ElectionGuard.Guardians;
using ElectionGuard.Decryption.ChallengeResponse;

namespace ElectionGuard.Decryption.Decryption;

public static class DecryptWithSharesExtensions
{
    public static PlaintextTally Decrypt(
        this CiphertextTally self,
        List<Tuple<ElectionPublicKey, TallyShare>> guardianShares,
        List<GuardianChallengeResponse> challengeResponses,
        bool skipValidation = false)
    {
        var lagrangeCoefficients = guardianShares.ComputeLagrangeCoefficients();
        return self.Decrypt(
            guardianShares,
            challengeResponses,
            lagrangeCoefficients,
            skipValidation
        );
    }

    public static PlaintextTally Decrypt(
        this CiphertextTally self,
        List<Tuple<ElectionPublicKey, TallyShare>> guardianShares,
        List<GuardianChallengeResponse> challengeResponses,
        Dictionary<string, LagrangeCoefficient> lagrangeCoefficients,
        bool skipValidation = false)
    {
        var accumulation = self.AccumulateShares(
            guardianShares, lagrangeCoefficients, skipValidation);

        accumulation!.AddProofs(
            self,
            challengeResponses,
            skipValidation
        );

        return self.Decrypt(accumulation, skipValidation);
    }

    public static PlaintextTally DecryptNoProofs(
        this CiphertextTally self,
        List<Tuple<ElectionPublicKey, TallyShare>> guardianShares,
        Dictionary<string, LagrangeCoefficient> lagrangeCoefficients)
    {
        var accumulation = self.AccumulateShares(
            guardianShares, lagrangeCoefficients, skipValidation: true);

        return self.Decrypt(accumulation, skipValidation: true);
    }

    public static PlaintextTally Decrypt(
        this CiphertextTally self,
        AccumulatedTally accumulation,
        bool skipValidation = false)
    {
        if (!skipValidation)
        {
            // iterate over the contests and make sure there are proofs
            foreach (var contest in self.Contests)
            {
                var contestAccumulation = accumulation.Contests.First(
                    x => x.Key == contest.Key).Value;

                foreach (var selection in contest.Value.Selections)
                {
                    var selectionAccumulation = contestAccumulation.Selections.First(
                        x => x.Key == selection.Key).Value;

                    if (selectionAccumulation.Value == null)
                    {
                        throw new Exception($"Selection value is null for {self.TallyId} {contest.Key} {selection.Key}");
                    }

                    if (selectionAccumulation.Proof == null)
                    {
                        throw new Exception($"Selection proof is null for {self.TallyId} {contest.Key} {selection.Key}");
                    }
                }
            }
        }

        return self.Decrypt(accumulation.Contests.Values.ToList());
    }

    public static PlaintextTally Decrypt(
        this CiphertextTally self,
        List<AccumulatedContest> accumulatedContests)
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

            var contestAccumulation = accumulatedContests.First(
                x => x.ObjectId == contest.Key);

            foreach (var selection in contest.Value.Selections)
            {
                // get the selection from the plaintext contest
                var plaintextSelection = plaintextContest.Selections.First(
                    x => x.Key == selection.Key).Value;

                var selectionAccumulation = contestAccumulation.Selections.First(
                    x => x.Key == selection.Key).Value;

                // decrypt the selection
                var ciphertext = selection.Value;
                var value = ciphertext!.Decrypt(
                    selectionAccumulation,
                    self.Context.ElGamalPublicKey);

                // add the decrypted value to the plaintext selection
                plaintextSelection.Tally += value.Tally;
                if (value.Proof != null)
                {
                    plaintextSelection.Proof = new(value.Proof);
                }
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
        Dictionary<string, List<BallotChallengeResponse>> challengeResponses,
        Dictionary<string, LagrangeCoefficient> lagrangeCoefficients,
        CiphertextTally tally,
        bool skipValidation = false)
    {
        var plaintextBallots = new List<PlaintextTallyBallot>();
        foreach (var (ballotId, ballot) in self)
        {
            var plaintextBallot = ballot.Decrypt(
                ballotShares[ballotId],
                challengeResponses[ballotId],
                lagrangeCoefficients,
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
        List<BallotChallengeResponse> challengeResponses,
        Dictionary<string, LagrangeCoefficient> lagrangeCoefficients,
        CiphertextTally tally,
        bool skipValidation = false)
    {
        return self.Decrypt(
            ballotShares.GetShares(),
            challengeResponses,
            lagrangeCoefficients,
            tally.TallyId,
            tally.Context,
            skipValidation);
    }

    /// <summary>
    /// Decrypt a single ballot using the provided ballot shares
    /// </summary>
    public static PlaintextTallyBallot Decrypt(
        this CiphertextBallot self,
        CiphertextDecryptionBallot ballotShares,
        List<BallotChallengeResponse> challengeResponses,
        Dictionary<string, LagrangeCoefficient> lagrangeCoefficients,
        string tallyId,
        CiphertextElectionContext context,
        bool skipValidation = false)
    {
        return self.Decrypt(
            ballotShares.GetShares(),
            challengeResponses,
            lagrangeCoefficients,
            tallyId,
            context,
            skipValidation);
    }

    /// <summary>
    /// Decrypt a single ballot using the provided ballot shares
    /// </summary>
    public static PlaintextTallyBallot Decrypt(
        this CiphertextBallot self,
        List<Tuple<ElectionPublicKey, BallotShare>> guardianShares,
        List<BallotChallengeResponse> challengeResponses,
        string tallyId,
        CiphertextElectionContext context,
        bool skipValidation = false)
    {
        var lagrangeCoefficients = guardianShares.ComputeLagrangeCoefficients();
        return self.Decrypt(
            guardianShares,
            challengeResponses,
            lagrangeCoefficients,
            tallyId,
            context, skipValidation
        );
    }

    /// <summary>
    /// Decrypt a single ballot using the provided ballot shares
    /// </summary>
    public static PlaintextTallyBallot Decrypt(
        this CiphertextBallot self,
        List<Tuple<ElectionPublicKey, BallotShare>> guardianShares,
        List<BallotChallengeResponse> challengeResponses,
        Dictionary<string, LagrangeCoefficient> lagrangeCoefficients,
        string tallyId,
        CiphertextElectionContext context,
        bool skipValidation = false)
    {
        // Accumulate the shares
        var accumulation = self.AccumulateShares(
            tallyId,
            guardianShares,
            lagrangeCoefficients,
             context.CryptoExtendedBaseHash,
             skipValidation);

        // Add the proofs
        accumulation.AddProofs(
            self,
            context,
            challengeResponses
        );

        return self.Decrypt(
            accumulation, tallyId, context, skipValidation);
    }

    public static PlaintextTallyBallot DecryptNoProofs(
        this CiphertextBallot self,
        List<Tuple<ElectionPublicKey, BallotShare>> guardianShares,
        Dictionary<string, LagrangeCoefficient> lagrangeCoefficients,
        string tallyId,
        CiphertextElectionContext context)
    {
        // Accumulate the shares
        var accumulation = self.AccumulateShares(
            tallyId,
            guardianShares,
            lagrangeCoefficients,
             context.CryptoExtendedBaseHash,
             skipValidation: true);

        return self.Decrypt(
            accumulation, tallyId, context, skipValidation: true);
    }

    /// <summary>
    /// Decrypt a single ballot using the provided ballot shares
    /// </summary>
    public static List<PlaintextTallyBallot> Decrypt(
        this Dictionary<string, CiphertextBallot> self,
        List<AccumulatedBallot> accumulations,
        string tallyId,
        CiphertextElectionContext context,
        bool skipValidation = false)
    {
        var plaintextBallots = new List<PlaintextTallyBallot>();
        foreach (var (ballotId, ballot) in self)
        {
            var plaintextBallot = ballot.Decrypt(
                accumulations.First(x => x.ObjectId == ballotId),
                tallyId,
                context,
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
        AccumulatedBallot accumulation,
        string tallyId,
        CiphertextElectionContext context,
        bool skipValidation = false)
    {
        if (!skipValidation)
        {
            if (accumulation.ObjectId != self.ObjectId)
            {
                throw new Exception("Ballot Id does not match");
            }

            foreach (var contest in self.Contests)
            {
                var contestAccumulation = accumulation.Contests.First(
                    x => x.Key == contest.ObjectId).Value;

                foreach (var selection in contest.Selections)
                {
                    var selectionAccumulation = contestAccumulation.Selections.First(
                        x => x.Key == selection.ObjectId).Value;

                    if (selectionAccumulation.Value == null)
                    {
                        throw new Exception($"Selection value is null for {self.ObjectId} {contest.ObjectId} {selection.ObjectId}");
                    }
                }
            }
        }

        return self.Decrypt(
            accumulation.Contests.Values.ToList(),
            context.ElGamalPublicKey,
            tallyId);
    }

    public static PlaintextTallyBallot Decrypt(
        this CiphertextBallot self,
        List<AccumulatedContest> accumulatedContests,
        ElementModP publicKey,
        string tallyId)
    {
        // create a plaintext tally from the first ballot share's style Id.
        var plaintextTally = new PlaintextTallyBallot(tallyId, self);

        // iterate over the contests from the ballot.
        foreach (var contest in self.Contests)
        {
            var plaintextContest = plaintextTally.Contests.First(
                x => x.Key == contest.ObjectId).Value;

            var contestAccumulation = accumulatedContests.First(
                x => x.ObjectId == contest.ObjectId);

            // iterate over the selections from the contest
            foreach (var selection in contest.Selections.Where(x => x.IsPlaceholder == false))
            {
                var plaintextSelection = plaintextContest.Selections.First(
                    x => x.Key == selection.ObjectId).Value;

                var selectionAcumulation = contestAccumulation.Selections.First(
                    x => x.Key == selection.ObjectId).Value;

                var value = selection.Decrypt(
                    selectionAcumulation, publicKey);
                plaintextSelection.Tally += value.Tally;
                if (value.Proof != null)
                {
                    plaintextSelection.Proof = new(value.Proof);
                }
            }
        }

        return plaintextTally;
    }

    /// <summary>
    /// Decrypt a single selection using the provided selection shares
    /// </summary>
    public static PlaintextTallySelection DecryptNoProofs(
        this CiphertextTallySelection self,
        List<Tuple<ElectionPublicKey, SelectionShare>> guardianShares,
        ElementModP publicKey,
        ElementModQ extendedBaseHash,
        bool skipValidation = false)
    {
        var lagrangeCoefficients = guardianShares
            .Select(x => x.Item1)
            .ToList()
            .ComputeLagrangeCoefficients();
        var decryption = self.AccumulateShares(
            guardianShares,
            lagrangeCoefficients,
            extendedBaseHash,
            skipValidation);
        return self.Decrypt(decryption, publicKey);
    }

    /// <summary>
    /// Decrypt a single selection using the provided selection shares
    /// </summary>
    public static PlaintextTallySelection DecryptNoProofs(
        this ICiphertextSelection self,
        List<Tuple<ElectionPublicKey, SelectionShare>> guardianShares,
        Dictionary<string, LagrangeCoefficient> lagrangeCoefficients,
        ElementModP publicKey,
        ElementModQ extendedBaseHash, bool skipValidation = false)
    {
        // accumulate all of the shares calculated for the selection
        var accumulation = self.AccumulateShares(
            guardianShares,
            lagrangeCoefficients,
            extendedBaseHash,
            skipValidation);
        return self.Decrypt(accumulation, publicKey);
    }

    /// <summary>
    /// Decrypt a single selection using the provided accumulated decryption.
    /// </summary>
    public static PlaintextTallySelection Decrypt(
        this ICiphertextSelection self,
        AccumulatedSelection accumulation, ElementModP publicKey)
    {
        // Calculate 𝑀=𝐵⁄(∏𝑀𝑖) mod 𝑝.
        var tally = self.Ciphertext.Decrypt(accumulation.Value, publicKey);
        if (!tally.HasValue)
        {
            throw new Exception("Failed to decrypt selection");
        }

        var plaintext = new PlaintextTallySelection(
            self, tally ?? 0UL, accumulation.Value!, accumulation.Proof!);
        return plaintext;
    }
}
