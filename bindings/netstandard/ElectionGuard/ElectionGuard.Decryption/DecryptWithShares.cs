
using ElectionGuard.Decryption.Decryption;
using ElectionGuard.Decryption.Tally;
using ElectionGuard.Encryption.Ballot;
using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.Decryption;

public static class DecryptWithSharesExtensions
{
    public static Dictionary<string, ElementModQ> ComputeLagrangeCoefficients(
        this List<Tuple<ElectionPublicKey, CiphertextDecryptionTallyShare>> guardianShares)
    {
        return ComputeLagrangeCoefficients(guardianShares.Select(x => x.Item1).ToList());
    }

    public static Dictionary<string, ElementModQ> ComputeLagrangeCoefficients(
        this List<Tuple<ElectionPublicKey, CiphertextDecryptionBallotShare>> guardianShares)
    {
        return ComputeLagrangeCoefficients(guardianShares.Select(x => x.Item1).ToList());
    }

    public static Dictionary<string, ElementModQ> ComputeLagrangeCoefficients(
        this List<ElectionPublicKey> guardians)
    {
        var lagrangeCoefficients = new Dictionary<string, ElementModQ>();
        foreach (var guardian in guardians)
        {
            var otherSequenceOrders = guardians
                .Where(i => i.OwnerId != guardian.OwnerId)
                .Select(x => x.SequenceOrder).ToList();
            var lagrangeCoefficient = Polynomial.Interpolate(
                guardian.SequenceOrder, otherSequenceOrders
                );
            lagrangeCoefficients.Add(guardian.OwnerId, lagrangeCoefficient);
        }
        return lagrangeCoefficients;
    }

    public static PlaintextTally Decrypt(
        this CiphertextTally self,
        List<Tuple<ElectionPublicKey, CiphertextDecryptionTallyShare>> guardianShares,
        bool skipValidation = false)
    {
        var lagrangeCoefficients = ComputeLagrangeCoefficients(guardianShares);

        return self.Decrypt(
            guardianShares, lagrangeCoefficients, skipValidation
        );
    }

    public static PlaintextTally Decrypt(
        this CiphertextTally self,
        List<Tuple<ElectionPublicKey, CiphertextDecryptionTallyShare>> guardianShares,
        Dictionary<string, ElementModQ> lagrangeCoefficients,
        bool skipValidation = false)
    {
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

        // Create a plaintext tally from the ciphertext tally
        var plaintextTally = new PlaintextTally(
            self.TallyId, self.Name, self.Manifest);

        // iterate over the ciphertext contests
        foreach (var contest in self.Contests)
        {
            // get the contest from the plaintext tally
            var plaintextContest = plaintextTally.Contests.First(
                x => x.Key == contest.Key).Value;

            foreach (var selection in contest.Value.Selections)
            {
                // get the selection from the plaintext contest
                var plaintextSelection = plaintextContest.Selections.First(
                    x => x.Key == selection.Key).Value;

                // get the selection shares from the guardian shares
                var selectionShares = guardianShares
                    .Select(
                        x => x.Item2.GetSelectionShare(x.Item1, contest.Key, selection.Key))
                    .ToList();

                // decrypt the selection
                var ciphertext = selection.Value;
                var value = ciphertext!.Decrypt(
                    selectionShares, lagrangeCoefficients,
                    self.Context.CryptoExtendedBaseHash, skipValidation);

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
        Dictionary<string, ElementModQ> lagrangeCoefficients,
        CiphertextTally tally,
        bool skipValidation = false)
    {
        var plaintextBallots = new List<PlaintextTallyBallot>();
        foreach (var (ballotId, ballot) in self)
        {
            var plaintextBallot = ballot.Decrypt(
                ballotShares[ballotId], lagrangeCoefficients,
                tally.TallyId, tally.Context.CryptoExtendedBaseHash, skipValidation);
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
        Dictionary<string, ElementModQ> lagrangeCoefficients,
        string tallyId,
        ElementModQ extendedBaseHash,
        bool skipValidation = false)
    {
        return self.Decrypt(
            ballotShares.GetShares(), lagrangeCoefficients, tallyId, extendedBaseHash, skipValidation);
    }

    /// <summary>
    /// Decrypt a single ballot using the provided ballot shares
    /// </summary>
    public static PlaintextTallyBallot Decrypt(
        this CiphertextBallot self,
        List<Tuple<ElectionPublicKey, CiphertextDecryptionBallotShare>> guardianShares,
        string tallyId,
        ElementModQ extendedBaseHash,
        bool skipValidation = false)
    {
        var lagrangeCoefficients = ComputeLagrangeCoefficients(guardianShares);

        return self.Decrypt(
            guardianShares, lagrangeCoefficients, tallyId, extendedBaseHash, skipValidation
        );
    }

    /// <summary>
    /// Decrypt a single ballot using the provided ballot shares
    /// </summary>
    public static PlaintextTallyBallot Decrypt(
        this CiphertextBallot self,
        List<Tuple<ElectionPublicKey, CiphertextDecryptionBallotShare>> guardianShares,
        Dictionary<string, ElementModQ> lagrangeCoefficients,
        string tallyId,
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

        // create a plaintext tally from the first ballot share's style Id.
        var firstShare = guardianShares.First().Item2;
        var styleId = firstShare.StyleId;
        var plaintextTally = new PlaintextTallyBallot(tallyId, self);

        // iterate over the contests from the ballot.
        foreach (var contest in self.Contests)
        {
            var plaintextContest = plaintextTally.Contests.First(
                x => x.Key == contest.ObjectId).Value;

            // iterate over the selections from the contest
            foreach (var selection in contest.Selections.Where(x => x.IsPlaceholder == false))
            {
                var plaintextSelection = plaintextContest.Selections.First(
                    x => x.Key == selection.ObjectId).Value;

                // get the selection shares from the guardian shares
                var selectionShares = guardianShares
                    .Select(
                    x => x.Item2.GetSelectionShare(
                        x.Item1, contest.ObjectId, selection.ObjectId))
                    .ToList();

                // decrypt the ciphertext
                var ciphertext = selection.Ciphertext;
                var value = selection!.Decrypt(
                    selectionShares, lagrangeCoefficients, extendedBaseHash, skipValidation);
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
        List<Tuple<ElectionPublicKey, CiphertextDecryptionSelectionShare>> guardianShares,
        ElementModQ extendedBaseHash,
        bool skipValidation = false)
    {
        var lagrangeCoefficients = ComputeLagrangeCoefficients(
            guardianShares.Select(x => x.Item1).ToList());
        return self.Decrypt(
            guardianShares,
            lagrangeCoefficients,
            extendedBaseHash,
            skipValidation);
    }

    /// <summary>
    /// Decrypt a single selection using the provided selection shares
    /// </summary>
    public static PlaintextTallySelection Decrypt(
        this ICiphertextSelection self,
        List<Tuple<ElectionPublicKey, CiphertextDecryptionSelectionShare>> guardianShares,
        Dictionary<string, ElementModQ> lagrangeCoefficients,
        ElementModQ extendedBaseHash, bool skipValidation = false)
    {
        if (!skipValidation)
        {
            foreach (var (guardian, share) in guardianShares)
            {
                if (!share.IsValid(self, guardian, extendedBaseHash))
                {
                    throw new Exception(
                        $"Failed to verify selection share for guardian {guardian.OwnerId}");
                }
            }
        }

        // accumulate all of the shares calculated for the selection
        var decryption = new CiphertextDecryptionSelection(self);

        // 𝑀𝑏𝑎𝑟 = 𝑀𝑏𝑎𝑟 * (𝑀𝑖 ^ 𝑤𝑖) mod p
        decryption.Accumulate(guardianShares, lagrangeCoefficients, skipValidation);
        Console.WriteLine($"Decryption: {decryption.Value}");

        // Calculate 𝑀=𝐵⁄(∏𝑀𝑖) mod 𝑝.
        var tally = self.Ciphertext.Decrypt(decryption.Value);
        if (!tally.HasValue)
        {
            throw new Exception("Failed to decrypt selection");
        }

        var plaintext = new PlaintextTallySelection(self, tally ?? 0UL, decryption.Value!);
        return plaintext;
    }
}
