
using ElectionGuard.Decryption.Tally;

namespace ElectionGuard.Decryption;

public static class DecryptExtensions
{
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
                var value = ciphertext.Decrypt(secretKey);
                var plaintextSelection = plaintextContest.Selections.First(
                    x => x.Key == selection.Key).Value;

                plaintextSelection.Tally += value ?? 0;
            }
        }
        return plaintextTally;
    }
}
