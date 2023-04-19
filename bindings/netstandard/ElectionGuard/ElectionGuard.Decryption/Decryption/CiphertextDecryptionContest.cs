using ElectionGuard.ElectionSetup;
using ElectionGuard.ElectionSetup.Extensions;
using ElectionGuard.Encryption.Ballot;

namespace ElectionGuard.Decryption.Decryption;

/// <summary>
/// The Decryption of a contest used when publishing the results of an election.
/// </summary>
public record CiphertextDecryptionContest
    : DisposableRecordBase, IElectionContest, IEquatable<CiphertextDecryptionContest>
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

    public Dictionary<string, CiphertextDecryptionSelection> Selections { get; init; } = default!;

    public CiphertextDecryptionContest(
        string objectId,
        ulong sequenceOrder,
        ElementModQ descriptionHash,
        Dictionary<string, CiphertextDecryptionSelection> selections)
    {
        ObjectId = objectId;
        SequenceOrder = sequenceOrder;
        DescriptionHash = new(descriptionHash);
        Selections = selections.Select(x => new CiphertextDecryptionSelection(x.Value)).ToDictionary(x => x.ObjectId);
    }

    public CiphertextDecryptionContest(IElectionContest contest,
    Dictionary<string, CiphertextDecryptionSelection> selections)
    {
        ObjectId = contest.ObjectId;
        SequenceOrder = contest.SequenceOrder;
        DescriptionHash = new(contest.DescriptionHash);
        Selections = selections.Select(x => new CiphertextDecryptionSelection(x.Value)).ToDictionary(x => x.ObjectId);
    }

    protected override void DisposeUnmanaged()
    {
        Selections.Dispose();
        DescriptionHash.Dispose();
        base.DisposeUnmanaged();
    }
}
