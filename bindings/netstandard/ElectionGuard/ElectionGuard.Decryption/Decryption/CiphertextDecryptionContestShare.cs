using ElectionGuard.ElectionSetup;
using ElectionGuard.Encryption.Ballot;

namespace ElectionGuard.Decryption.Decryption;

public record CiphertextDecryptionContestShare
    : DisposableRecordBase, IElectionContest, IEquatable<CiphertextDecryptionContestShare>
{
    /// <summary>
    /// The object id of the contest
    /// </summary>
    public string ObjectId { get; init; }

    /// <summary>
    /// The sequence order of the contest
    /// </summary>
    public ulong SequenceOrder { get; init; }

    /// <summary>
    /// The hash of the contest description
    /// </summary>
    public ElementModQ DescriptionHash { get; init; }

    public Dictionary<string, CiphertextDecryptionSelectionShare> Selections { get; init; } = default!;

    public CiphertextDecryptionContestShare(
        string objectId,
        ulong sequenceOrder,
        ElementModQ descriptionHash,
        Dictionary<string, CiphertextDecryptionSelectionShare> selections)
    {
        ObjectId = objectId;
        SequenceOrder = sequenceOrder;
        DescriptionHash = descriptionHash;
        Selections = selections;
    }

    public CiphertextDecryptionContestShare(IElectionContest contest,
    Dictionary<string, CiphertextDecryptionSelectionShare> selections)
    {
        ObjectId = contest.ObjectId;
        SequenceOrder = contest.SequenceOrder;
        DescriptionHash = contest.DescriptionHash;
        Selections = selections;
    }
}
