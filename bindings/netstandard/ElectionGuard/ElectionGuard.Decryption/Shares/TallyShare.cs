using ElectionGuard.Decryption.Tally;
using ElectionGuard.ElectionSetup;
using ElectionGuard.ElectionSetup.Extensions;
using ElectionGuard.Guardians;

namespace ElectionGuard.Decryption.Shares;

/// <summary>
/// a share of a guardian's decryption of a collection of contests that have been accumulated into a tally
/// </summary>
public record TallyShare : DisposableRecordBase, IEquatable<TallyShare>
{
    public string GuardianId { get; init; }

    public string TallyId { get; init; }

    public Dictionary<string, ContestShare> Contests { get; init; } = default!;

    public TallyShare(
        string guardianId,
        string tallyId,
        Dictionary<string, ContestShare> contests)
    {
        GuardianId = guardianId;
        TallyId = tallyId;
        Contests = contests.Select(
            x => new KeyValuePair<string, ContestShare>(x.Key, new(x.Value)))
        .ToDictionary(x => x.Key, x => x.Value);
    }

    public TallyShare(
        TallyShare other)
        : base(other)
    {
        GuardianId = other.GuardianId;
        TallyId = other.TallyId;
        Contests = other.Contests.Select(
            x => new KeyValuePair<string, ContestShare>(x.Key, new(x.Value)))
        .ToDictionary(x => x.Key, x => x.Value);
    }

    public virtual bool IsValid(
        CiphertextTally tally, ElectionPublicKey guardian)
    {
        if (guardian.GuardianId != GuardianId)
        {
            return false;
        }

        if (tally.TallyId != TallyId)
        {
            return false;
        }

        foreach (var contest in tally.Contests)
        {
            if (!Contests.ContainsKey(contest.Key))
            {
                return false;
            }

            if (!Contests[contest.Key].IsValid(
                contest.Value,
                guardian,
                tally.Context.CryptoExtendedBaseHash))
            {
                return false;
            }
        }

        return true;
    }

    public Tuple<ElectionPublicKey, ContestShare> GetContestShare(
        ElectionPublicKey guardian,
        string contestId)
    {
        var contest = GetContestShare(guardian.GuardianId, contestId);
        return new Tuple<ElectionPublicKey, ContestShare>(guardian, contest);
    }

    public ContestShare GetContestShare(
        string guardianId, string contestId)
    {
        return guardianId != GuardianId
            ? throw new ArgumentException($"GuardianId {guardianId} does not match {GuardianId}")
            : Contests[contestId];
    }

    public Tuple<ElectionPublicKey, SelectionShare> GetSelectionShare(
        ElectionPublicKey guardian,
        string contestId, string selectionId)
    {
        var selection = GetSelectionShare(guardian.GuardianId, contestId, selectionId);
        return new Tuple<ElectionPublicKey, SelectionShare>(guardian, selection);
    }

    public SelectionShare GetSelectionShare(
        string guardianId, string contestId, string selectionId)
    {
        return guardianId != GuardianId
            ? throw new ArgumentException($"GuardianId {guardianId} does not match {GuardianId}")
            : Contests[contestId].Selections[selectionId];
    }

    protected override void DisposeManaged()
    {
        base.DisposeManaged();
        Contests.Dispose();
    }
}
