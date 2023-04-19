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

    public static PlaintextTallyBallot ToTallyBallot(this PlaintextBallot ballot, CiphertextTally tally)
    {
        var plaintext = new PlaintextTallyBallot(tally.TallyId, ballot.ObjectId, ballot.StyleId, tally.Manifest);
        foreach (var (contestId, contest) in plaintext.Contests)
        {
            var plaintextContest = ballot.Contests.First(i => i.ObjectId == contestId);
            foreach (var (selectionId, selection) in contest.Selections)
            {
                var plaintextSelection = plaintextContest.Selections.First(i => i.ObjectId == selectionId);
                selection.Tally = plaintextSelection.Vote;
                // TODO: add support for extended data
            }
        }

        return plaintext;
    }
}
