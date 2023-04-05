
using ElectionGuard.Decryption.Decryption;
using ElectionGuard.Decryption.Tally;
using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.Decryption;

public static class DecryptWithSharesExtensions
{
    public static List<PlaintextTallyBallot> Decrypt(
        this CiphertextTally self,
        Dictionary<string, CiphertextDecryptionBallot> ballotShares,
        ElementModQ extendedBaseHash,
        bool skipValidation = false)
    {
        var plaintextBallots = new List<PlaintextTallyBallot>();
        foreach (var ballotShare in ballotShares)
        {
            var plaintextBallot = self.Decrypt(
                ballotShare.Value, extendedBaseHash, skipValidation);
            plaintextBallots.Add(plaintextBallot);
        }

        return plaintextBallots;
    }

    public static PlaintextTallyBallot Decrypt(
        this CiphertextTally self,
        CiphertextDecryptionBallot ballotShares,
        ElementModQ extendedBaseHash,
        bool skipValidation = false)
    {
        return self.Decrypt(
            ballotShares.GetShares(), extendedBaseHash, skipValidation);
    }

    public static PlaintextTally Decrypt(
        this CiphertextTally self,
        List<Tuple<ElectionPublicKey, CiphertextDecryptionTallyShare>> guardianShares,
        ElementModQ extendedBaseHash,
        bool skipValidation = false)
    {
        var plaintextTally = new PlaintextTally(
            self.TallyId, self.Name, self.Manifest);

        var lagrangeCoefficients = ComputeLagrangeCoefficients(guardianShares);

        foreach (var contest in self.Contests)
        {
            var plaintextContest = plaintextTally.Contests.First(
                x => x.Key == contest.Key).Value;

            foreach (var selection in contest.Value.Selections)
            {
                var ciphertext = selection.Value;
                var plaintextSelection = plaintextContest.Selections.First(
                    x => x.Key == selection.Key).Value;

                var selectionShares = guardianShares.Select(
                    x => new Tuple<ElectionPublicKey, CiphertextDecryptionSelectionShare>(
                        x.Item1, x.Item2.GetSelectionShare(contest.Key, selection.Key))).ToList();

                var value = ciphertext!.Decrypt(
                    selectionShares, lagrangeCoefficients, extendedBaseHash, skipValidation);
                plaintextSelection.Tally += value.Tally;

            }
        }

        return plaintextTally;
    }

    public static PlaintextTallyBallot Decrypt(
        this CiphertextTally self,
        List<Tuple<ElectionPublicKey, CiphertextDecryptionBallotShare>> guardianShares,
        ElementModQ extendedBaseHash,
        bool skipValidation = false)
    {
        var firstShare = guardianShares.First().Item2;
        var ballotStyleId = firstShare.StyleId;
        var plaintextTally = new PlaintextTallyBallot(
            self.TallyId, firstShare.BallotId, ballotStyleId, self.Manifest);

        // TODO: move this up the chain
        var lagrangeCoefficients = ComputeLagrangeCoefficients(guardianShares);

        foreach (var contest in self.Contests)
        {
            var plaintextContest = plaintextTally.Contests.First(
                x => x.Key == contest.Key).Value;

            foreach (var selection in contest.Value.Selections)
            {
                var ciphertext = selection.Value;
                var plaintextSelection = plaintextContest.Selections.First(
                    x => x.Key == selection.Key).Value;

                var selectionShares = guardianShares.Select(
                    x => new Tuple<ElectionPublicKey, CiphertextDecryptionSelectionShare>(
                        x.Item1, x.Item2.GetSelectionShare(contest.Key, selection.Key))).ToList();

                var value = ciphertext!.Decrypt(
                    selectionShares, lagrangeCoefficients, extendedBaseHash, skipValidation);
                plaintextSelection.Tally += value.Tally;

            }
        }

        return plaintextTally;
    }

    public static Dictionary<string, ElementModQ> ComputeLagrangeCoefficients(
        List<Tuple<ElectionPublicKey, CiphertextDecryptionTallyShare>> guardianShares)
    {
        return ComputeLagrangeCoefficients(guardianShares.Select(x => x.Item1).ToList());
    }

    public static Dictionary<string, ElementModQ> ComputeLagrangeCoefficients(
        List<Tuple<ElectionPublicKey, CiphertextDecryptionBallotShare>> guardianShares)
    {
        return ComputeLagrangeCoefficients(guardianShares.Select(x => x.Item1).ToList());
    }

    public static Dictionary<string, ElementModQ> ComputeLagrangeCoefficients(
        List<ElectionPublicKey> guardians)
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

    // TODO: a better override that also accepts the lagrange coeffieints so we dont need to recalculate them
    public static PlaintextTallySelection Decrypt(
        this CiphertextTallySelection self,
        List<Tuple<ElectionPublicKey, CiphertextDecryptionSelectionShare>> guardianShares,
        ElementModQ extendedBaseHash,
        bool skipValidation = false)
    {
        var lagrangeCoefficients = ComputeLagrangeCoefficients(guardianShares.Select(x => x.Item1).ToList());
        return self.Decrypt(guardianShares, lagrangeCoefficients, extendedBaseHash, skipValidation);
    }

    public static PlaintextTallySelection Decrypt(
        this CiphertextTallySelection self,
        List<Tuple<ElectionPublicKey, CiphertextDecryptionSelectionShare>> guardianShares,
        Dictionary<string, ElementModQ> lagrangeCoefficients,
        ElementModQ extendedBaseHash, bool skipValidation = false)
    {
        if (!skipValidation)
        {
            foreach (var (guardian, share) in guardianShares)
            {
                if (!share.IsValid(self.Ciphertext, guardian, extendedBaseHash))
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
