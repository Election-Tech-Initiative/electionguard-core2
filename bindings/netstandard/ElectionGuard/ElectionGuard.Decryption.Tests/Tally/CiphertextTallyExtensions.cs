using ElectionGuard.Decryption.Tally;

namespace ElectionGuard.Decryption.Tests.Tally;

public static class CiphertextTallyExtensions
{
    // accumulate plaintext ballots into a plaintext tally.
    // Useful for comparisons of actual results vs expected results
    public static void AccumulateBallots(
        this PlaintextTally self, IList<PlaintextBallot> ballots)
    {
        Console.WriteLine($"Accumulating {ballots.Count} ballots");
        var contestVotes = new Dictionary<string, int>();
        foreach (var contest in self.Contests)
        {
            contestVotes[contest.Key] = 0;
        }
        foreach (var ballot in ballots)
        {
            foreach (var contest in ballot.Contests)
            {
                contestVotes[contest.ObjectId] += 1;
                var contestTally = self.Contests[contest.ObjectId];
                foreach (var selection in contest.Selections)
                {
                    var selectionTally = contestTally.Selections[selection.ObjectId];
                    selectionTally.Tally += selection.Vote;
                }
            }
        }

        foreach (var item in contestVotes)
        {
            Console.WriteLine($"    Contest {item.Key} has {item.Value} ballots");
        }
    }
}
