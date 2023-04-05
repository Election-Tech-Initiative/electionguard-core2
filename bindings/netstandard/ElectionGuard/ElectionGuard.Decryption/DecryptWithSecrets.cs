
using ElectionGuard.Decryption.Decryption;
using ElectionGuard.Decryption.Tally;
using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.Decryption;

public static class DecryptWithSecretsExtensions
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

                var value = ciphertext.Decrypt(secretKey);
                plaintextSelection.Tally += value ?? 0;
            }
        }
        return plaintextTally;
    }

    // should not be uised in production
    public static PlaintextTallyBallot Decrypt(
        this CiphertextBallot self,
        InternalManifest manifest,
        ElementModQ secretKey)
    {
        var plaintextBallot = new PlaintextTallyBallot(
            self.ObjectId, self.ObjectId, self.StyleId, manifest);

        foreach (var contest in self.Contests)
        {
            var plaintextContest = plaintextBallot.Contests.First(
                x => x.Key == contest.ObjectId).Value;
            foreach (var selection in contest.Selections.Where(x => x.IsPlaceholder == false))
            {
                var ciphertext = selection.Ciphertext;
                var plaintextSelection = plaintextContest.Selections.First(
                    x => x.Key == selection.ObjectId).Value;

                var value = ciphertext.Decrypt(secretKey);
                plaintextSelection.Tally += value ?? 0;

            }
        }
        return plaintextBallot;
    }
}
