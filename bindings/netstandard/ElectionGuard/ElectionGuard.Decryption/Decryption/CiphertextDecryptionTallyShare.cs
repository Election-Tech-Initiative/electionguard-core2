using ElectionGuard.Decryption.Tally;
using ElectionGuard.ElectionSetup;
using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.Decryption.Decryption;

/// <summary>
/// a share of a guardian's decryption of a collection of contests that have been accumulated into a tally
/// </summary>
public record CiphertextDecryptionTallyShare : DisposableRecordBase, IEquatable<CiphertextDecryptionTallyShare>
{
    public string GuardianId { get; init; }

    public string TallyId { get; init; }

    public Dictionary<string, CiphertextDecryptionContestShare> Contests { get; init; } = default!;

    public CiphertextDecryptionTallyShare(
        string guardianId,
        string tallyId,
        Dictionary<string, CiphertextDecryptionContestShare> contests)
    {
        GuardianId = guardianId;
        TallyId = tallyId;
        Contests = contests.Select(
            x => new KeyValuePair<string, CiphertextDecryptionContestShare>(x.Key, new(x.Value))).ToDictionary(x => x.Key, x => x.Value);
    }

    public CiphertextDecryptionTallyShare(CiphertextDecryptionTallyShare other) : base(other)
    {
        GuardianId = other.GuardianId;
        TallyId = other.TallyId;
        Contests = other.Contests.Select(
            x => new KeyValuePair<string, CiphertextDecryptionContestShare>(x.Key, new(x.Value))).ToDictionary(x => x.Key, x => x.Value);
    }

    public virtual bool IsValid(
        CiphertextTally tally, ElectionPublicKey guardian)
    {
        if (guardian.OwnerId != GuardianId)
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

    public Tuple<ElectionPublicKey, CiphertextDecryptionSelectionShare> GetSelectionShare(
        ElectionPublicKey guardian,
        string contestId, string selectionId)
    {
        var selection = GetSelectionShare(guardian.OwnerId, contestId, selectionId);
        return new Tuple<ElectionPublicKey, CiphertextDecryptionSelectionShare>(guardian, selection);
    }

    public CiphertextDecryptionSelectionShare GetSelectionShare(
        string guardianId, string contestId, string selectionId)
    {
        return guardianId != GuardianId
            ? throw new ArgumentException($"GuardianId {guardianId} does not match {GuardianId}")
            : Contests[contestId].Selections[selectionId];
    }

    protected override void DisposeUnmanaged()
    {
        foreach (var contest in Contests.Values)
        {
            contest.Dispose();
        }
        base.DisposeUnmanaged();
    }
}
