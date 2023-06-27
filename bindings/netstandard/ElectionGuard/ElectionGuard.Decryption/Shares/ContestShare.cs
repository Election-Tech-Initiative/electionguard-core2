using ElectionGuard.Ballot;
using ElectionGuard.Decryption.Tally;
using ElectionGuard.ElectionSetup;
using ElectionGuard.ElectionSetup.Extensions;
using ElectionGuard.Guardians;

namespace ElectionGuard.Decryption.Shares;

/// <summary>
/// A Guardian's Partial Decryption of a contest. 
/// This object is used both for Tally's and Ballot partial decryptions.
/// </summary>
public record ContestShare
    : DisposableRecordBase, IElectionContest, IEquatable<ContestShare>
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

    // partial decryption of extended data for the contest
    public ElementModP? ExtendedData { get; init; } = default!;

    /// <summary>
    /// Collection of Selection Shares
    /// the key is the selection object id
    /// </summary>
    public Dictionary<string, SelectionShare> Selections { get; init; } = default!;

    public ContestShare(
        IElectionContest contest,
        Dictionary<string, SelectionShare> selections)
    {
        ObjectId = contest.ObjectId;
        SequenceOrder = contest.SequenceOrder;
        DescriptionHash = new(contest.DescriptionHash);
        Selections = selections.Select(
            x => new SelectionShare(x.Value))
            .ToDictionary(x => x.ObjectId);
    }

    public ContestShare(
        IElectionContest contest,
        ElementModP extendedData,
        Dictionary<string, SelectionShare> selections)
    {
        ObjectId = contest.ObjectId;
        SequenceOrder = contest.SequenceOrder;
        DescriptionHash = new(contest.DescriptionHash);
        ExtendedData = new(extendedData);
        Selections = selections.Select(
            x => new SelectionShare(x.Value))
            .ToDictionary(x => x.ObjectId);
    }

    [JsonConstructor]
    public ContestShare(
        string objectId,
        ulong sequenceOrder,
        ElementModQ descriptionHash,
        ElementModP extendedData,
        Dictionary<string, SelectionShare> selections)
    {
        ObjectId = objectId;
        SequenceOrder = sequenceOrder;
        DescriptionHash = new(descriptionHash);
        ExtendedData = extendedData != null ? new(extendedData) : null;
        Selections = selections.Select(
            x => new SelectionShare(x.Value))
            .ToDictionary(x => x.ObjectId);
    }

    public ContestShare(
        IElectionContest contest,
        ElementModQ descriptionHash,
        ElementModP extendedData,
        Dictionary<string, SelectionShare> selections)
    {
        ObjectId = contest.ObjectId;
        SequenceOrder = contest.SequenceOrder;
        DescriptionHash = new(descriptionHash);
        ExtendedData = new(extendedData);
        Selections = selections.Select(
            x => new SelectionShare(x.Value))
            .ToDictionary(x => x.ObjectId);
    }

    public ContestShare(ContestShare other) : base(other)
    {
        ObjectId = other.ObjectId;
        SequenceOrder = other.SequenceOrder;
        DescriptionHash = new(other.DescriptionHash);
        ExtendedData = other.ExtendedData != null
            ? new(other.ExtendedData) : null;
        Selections = other.Selections.Select(
            x => new SelectionShare(x.Value))
            .ToDictionary(x => x.ObjectId);
    }

    /// <summary>
    /// Verify the validity of the contest share against a ballot.
    /// </summary>
    public bool IsValid(
        CiphertextBallotContest contest,
        ElectionPublicKey guardian,
        ElementModQ extendedBaseHash)
    {
        if (contest.ObjectId != ObjectId)
        {
            return false;
        }

        // TODO: verify the extended data

        // validate each selection
        foreach (var selection in contest.Selections)
        {
            if (!Selections.ContainsKey(selection.ObjectId))
            {
                return false;
            }

            if (!Selections[selection.ObjectId].IsValid(
                selection, guardian))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Verify the validity of the contest share against a tally.
    /// </summary>
    public bool IsValid(
        CiphertextTallyContest contest,
        ElectionPublicKey guardian,
        ElementModQ extendedBaseHash)
    {
        if (contest.ObjectId != ObjectId)
        {
            return false;
        }

        // validate each selection
        foreach (var selection in contest.Selections)
        {
            if (!Selections.ContainsKey(selection.Key))
            {
                return false;
            }

            if (!Selections[selection.Key].IsValid(
                selection.Value, guardian))
            {
                return false;
            }
        }
        return true;
    }

    protected override void DisposeManaged()
    {
        base.DisposeManaged();
        Selections.Dispose();
    }

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();
        ExtendedData?.Dispose();
        DescriptionHash?.Dispose();
    }
}
