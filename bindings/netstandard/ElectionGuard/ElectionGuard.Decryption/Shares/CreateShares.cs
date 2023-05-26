using ElectionGuard.Ballot;
using ElectionGuard.Decryption.Tally;
using ElectionGuard.ElectionSetup;

namespace ElectionGuard.Decryption.Shares;

/// <summary>
/// Decryption extension methods for the <see cref="Guardian" /> class
/// </summary>
public static class CreateSharesExtensions
{
    /// <summary>
    /// Conmpute decryption shares for a tally and a list of ballots. 
    /// Usially the list of ballots is the challenged ballots in the tally.
    /// </summary>
    public static DecryptionShare ComputeDecryptionShares(
        this Guardian guardian,
        CiphertextTally tally,
        List<CiphertextBallot> ballots)
    {
        var share = guardian.ComputeDecryptionShare(tally)!;
        var shares = guardian.ComputeDecryptionShares(tally.TallyId, ballots)!;
        return new(share, shares);
    }

    /// <summary>
    /// Compute decryption shares for a list of ballots.
    /// </summary>
    public static Dictionary<string, BallotShare> ComputeDecryptionShares(
        this Guardian guardian,
        string tallyId,
        List<CiphertextBallot> ballots)
    {
        var shares = new Dictionary<string, BallotShare>();
        foreach (var ballot in ballots)
        {
            shares.Add(
                ballot.ObjectId, guardian.ComputeDecryptionShare(tallyId, ballot)!);
        }

        return shares;
    }

    /// <summary>
    /// Compute a decryption share for a tally
    /// </summary>
    public static TallyShare? ComputeDecryptionShare(
        this Guardian guardian,
        CiphertextTally tally)
    {
        var contests = new Dictionary<string, ContestShare>();
        foreach (var contest in tally.Contests.Values)
        {
            contests.Add(contest.ObjectId, guardian.ComputeDecryptionShare(contest)!);
        }

        var share = new TallyShare(
            guardian.GuardianId, tally.TallyId, contests
        );
        return share;
    }

    /// <summary>
    /// Compute a decryption share for a ballot
    /// </summary>
    public static BallotShare? ComputeDecryptionShare(
        this Guardian guardian,
        string tallyId,
        CiphertextBallot ballot)
    {
        var contests = new Dictionary<string, ContestShare>();
        foreach (var contest in ballot.Contests)
        {
            contests.Add(
                contest.ObjectId, guardian.ComputeDecryptionShare(contest)!);
        }

        var share = new BallotShare(
            guardian.GuardianId, tallyId, ballot, contests
        );
        return share;
    }

    /// <summary>
    /// Compute a decryption share for a contest on a tally
    /// </summary>
    public static ContestShare ComputeDecryptionShare(
        this Guardian guardian,
        CiphertextTallyContest contest)
    {
        var selections = new Dictionary<string, SelectionShare>();
        foreach (var selection in contest.Selections.Values)
        {
            selections.Add(
                selection.ObjectId, guardian.ComputeDecryptionShare(selection)!);
        }

        var share = new ContestShare(
            contest, selections);
        return share;
    }

    /// <summary>
    /// Compute a decryption share for a contest on a ballot
    /// </summary>
    public static ContestShare ComputeDecryptionShare(
        this Guardian guardian,
        CiphertextBallotContest contest)
    {
        var extendedData = guardian.PartialDecrypt(contest.ExtendedData);

        var selections = new Dictionary<string, SelectionShare>();
        foreach (var selection in contest.Selections)
        {
            selections.Add(
                selection.ObjectId, guardian.ComputeDecryptionShare(selection)!);
        }

        var share = new ContestShare(
            contest, extendedData, selections);
        return share;
    }

    /// <summary>
    /// Compute a decryption share for a selection on a tally or a ballot.
    /// </summary>
    public static SelectionShare ComputeDecryptionShare(
        this Guardian guardian,
        ICiphertextSelection selection)
    {
        // 𝑀𝑖 = 𝐴^𝑠𝑖 mod 𝑝 
        var partial = guardian.PartialDecrypt(selection.Ciphertext);
        var commitment = guardian.CreateCommitment(selection);

        var share = new SelectionShare(
            selection,
            guardian.GuardianId,
            partial,
            commitment);
        return share;
    }
}
