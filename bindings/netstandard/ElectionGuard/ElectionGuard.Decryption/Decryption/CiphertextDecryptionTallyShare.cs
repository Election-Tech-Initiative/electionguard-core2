using ElectionGuard.Decryption.Tally;
using ElectionGuard.ElectionSetup;

namespace ElectionGuard.Decryption.Decryption;

// a share of a guardian's decryption of a collection of contests (usually the tally)
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
        Contests = contests;
    }

    public bool IsValid(CiphertextTally tally)
    {
        // TODO: implement
        return true;
    }

    public CiphertextDecryptionSelectionShare GetSelectionShare(string contestId, string selectionId)
    {
        return Contests[contestId].Selections[selectionId];
    }
}
