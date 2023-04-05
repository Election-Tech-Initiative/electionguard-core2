
using ElectionGuard.Decryption.Decryption;
using ElectionGuard.Decryption.Tally;
using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.Decryption;

public static class DecryptExtensions
{
    /// <summary>
    /// Decrypts a <see cref="CiphertextTally" /> using the provided <see cref="ElementModQ" /> secret key
    /// </summary>
    public static PlaintextTally Decrypt(
        this CiphertextTally self,
        ElementModQ secretKey)
    {
        var plaintextTally = new PlaintextTally(
            self.TallyId, self.Name, self.Manifest);

        foreach (var contest in self.Contests)
        {
            var plaintextContest = plaintextTally.Contests.First(
                x => x.Key == contest.Key).Value;
            foreach (var selection in contest.Value.Selections)
            {
                var ciphertext = selection.Value.Ciphertext;
                var plaintextSelection = plaintextContest.Selections.First(
                    x => x.Key == selection.Key).Value;

                try
                {
                    var value = ciphertext.Decrypt(secretKey);
                    plaintextSelection.Tally += value ?? 0;
                }
                catch (Exception e)
                {
                    throw new Exception(
                        $"Failed to decrypt selection {selection.Key} in contest {contest.Key}",
                        e);
                }
            }
        }
        return plaintextTally;
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

    public static Dictionary<string, ElementModQ> ComputeLagrangeCoefficients(
        List<Tuple<ElectionPublicKey, CiphertextDecryptionTallyShare>> guardianShares)
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
