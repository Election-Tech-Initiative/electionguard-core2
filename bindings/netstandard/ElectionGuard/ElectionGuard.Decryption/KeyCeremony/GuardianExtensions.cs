using ElectionGuard.Decryption.Decryption;
using ElectionGuard.Decryption.Tally;
using ElectionGuard.ElectionSetup;
using ElectionGuard.Encryption.Ballot;

namespace ElectionGuard.Decryption.KeyCeremony;

public static class GuardianDecryptionExtensions
{
    public static Tuple<CiphertextDecryptionTallyShare, CiphertextDecryptionBallotShares> ComputeDecryptionShares(
        this Guardian guardian,
        CiphertextTally tally, List<CiphertextBallot> ballots)
    {
        var share = guardian.ComputeDecryptionShare(tally)!;
        var shares = guardian.ComputeDecryptionShares(tally.TallyId, ballots)!;
        return new Tuple<CiphertextDecryptionTallyShare, CiphertextDecryptionBallotShares>(share, shares);
    }

    public static CiphertextDecryptionBallotShares? ComputeDecryptionShares(
        this Guardian guardian,
        string tallyId,
        List<CiphertextBallot> ballots)
    {
        var shares = new Dictionary<string, CiphertextDecryptionBallotShare>();
        foreach (var ballot in ballots)
        {
            shares.Add(ballot.ObjectId, guardian.ComputeDecryptionShare(tallyId, ballot)!);
        }

        var share = new CiphertextDecryptionBallotShares(shares);
        return share;
    }

    public static CiphertextDecryptionTallyShare? ComputeDecryptionShare(
        this Guardian guardian,
        CiphertextTally tally)
    {
        var contests = new Dictionary<string, CiphertextDecryptionContestShare>();
        foreach (var contest in tally.Contests.Values)
        {
            contests.Add(contest.ObjectId, guardian.ComputeDecryptionShare(contest)!);
        }

        var share = new CiphertextDecryptionTallyShare(
            guardian.GuardianId, tally.TallyId, contests
        );
        return share;
    }

    public static CiphertextDecryptionBallotShare? ComputeDecryptionShare(
        this Guardian guardian,
        string tallyId,
        CiphertextBallot ballot)
    {
        var contests = new Dictionary<string, CiphertextDecryptionContestShare>();
        foreach (var contest in ballot.Contests)
        {
            contests.Add(contest.ObjectId, guardian.ComputeDecryptionShare(contest)!);
        }

        var share = new CiphertextDecryptionBallotShare(
            guardian.GuardianId, tallyId, ballot.ObjectId, contests
        );
        return share;
    }

    public static CiphertextDecryptionContestShare ComputeDecryptionShare(
        this Guardian guardian,
        CiphertextTallyContest contest)
    {
        var selections = new Dictionary<string, CiphertextDecryptionSelectionShare>();
        foreach (var selection in contest.Selections.Values)
        {
            selections.Add(selection.ObjectId, guardian.ComputeDecryptionShare(selection)!);
        }

        var share = new CiphertextDecryptionContestShare(
            contest, selections);
        return share;
    }

    public static CiphertextDecryptionContestShare ComputeDecryptionShare(
        this Guardian guardian,
        CiphertextBallotContest contest)
    {
        var selections = new Dictionary<string, CiphertextDecryptionSelectionShare>();
        foreach (var selection in contest.Selections)
        {
            selections.Add(selection.ObjectId, guardian.ComputeDecryptionShare(selection)!);
        }

        var share = new CiphertextDecryptionContestShare(
            contest, selections);
        return share;
    }

    public static CiphertextDecryptionSelectionShare ComputeDecryptionShare(
        this Guardian guardian,
        ICiphertextSelection selection)
    {
        var partial = guardian.PartialDecrypt(selection.Ciphertext);
        // TODO: real proof
        var proof = new ChaumPedersenProof();

        var share = new CiphertextDecryptionSelectionShare(
            selection, guardian.GuardianId, partial, proof);
        return share;
    }
}
