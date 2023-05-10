using ElectionGuard.Ballot;
using ElectionGuard.Decryption.Shares;
using ElectionGuard.ElectionSetup;
using ElectionGuard.ElectionSetup.Extensions;
using ElectionGuard.Guardians;

namespace ElectionGuard.Decryption.Accumulation;

/// <summary>
/// The Decryption of a contest used when publishing the results of an election.
/// </summary>
public record AccumulatedContest
    : DisposableRecordBase, IElectionContest, IEquatable<AccumulatedContest>
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

    /// <summary>
    /// The accumulated selections for this contest
    /// </summary>
    public Dictionary<string, AccumulatedSelection> Selections { get; init; } = default!;

    public AccumulatedContest(
        string objectId,
        ulong sequenceOrder,
        ElementModQ descriptionHash,
        Dictionary<string, AccumulatedSelection> selections)
    {
        ObjectId = objectId;
        SequenceOrder = sequenceOrder;
        DescriptionHash = new(descriptionHash);
        Selections = selections.Select(
            x => new AccumulatedSelection(x.Value))
        .ToDictionary(x => x.ObjectId);
    }

    public AccumulatedContest(
        IElectionContest contest,
        IList<IElectionSelection> selections)
    {
        ObjectId = contest.ObjectId;
        SequenceOrder = contest.SequenceOrder;
        DescriptionHash = new(contest.DescriptionHash);
        Selections = selections.Select(
            x => new AccumulatedSelection(x))
        .ToDictionary(x => x.ObjectId);
    }

    public AccumulatedContest(
        IElectionContest contest,
        Dictionary<string, AccumulatedSelection> selections)
    {
        ObjectId = contest.ObjectId;
        SequenceOrder = contest.SequenceOrder;
        DescriptionHash = new(contest.DescriptionHash);
        Selections = selections.Select(
            x => new AccumulatedSelection(x.Value))
        .ToDictionary(x => x.ObjectId);
    }

    public AccumulatedContest(
        AccumulatedContest other) : base(other)
    {
        ObjectId = other.ObjectId;
        SequenceOrder = other.SequenceOrder;
        DescriptionHash = new(other.DescriptionHash);
        Selections = other.Selections.Select(
            x => new AccumulatedSelection(x.Value))
        .ToDictionary(x => x.ObjectId);
    }

    /// <summary>
    /// Accumulate the shares for this contest
    /// </summary>
    public void Accumulate(
        List<Tuple<ElectionPublicKey, ContestShare>> guardianShares,
        Dictionary<string, LagrangeCoefficient> lagrangeCoefficients,
        bool skipValidation = false
    )
    {
        foreach (var (guardianPublicKey, share) in guardianShares)
        {
            if (!lagrangeCoefficients.TryGetValue(guardianPublicKey.GuardianId, out var lagrangeCoefficient))
            {
                throw new KeyNotFoundException(
                    $"lagrangeCoefficient not found for guardian {guardianPublicKey.GuardianId}");
            }

            Accumulate(share, lagrangeCoefficient, skipValidation);
        }
    }

    /// <summary>
    /// Accumulate the shares for this contest

    public void Accumulate(
        ContestShare share,
        LagrangeCoefficient lagrangeCoefficient, bool skipValidation)
    {
        if (!skipValidation)
        {
            // TODO: validate?
        }

        foreach (var (selectionId, selectionShare) in share.Selections)
        {
            if (!Selections.TryGetValue(selectionId, out var selection))
            {
                throw new KeyNotFoundException(
                    $"selectionId not found for selection {selectionId}");
            }

            selection.Accumulate(selectionShare, lagrangeCoefficient, skipValidation);
        }
    }

    protected override void DisposeManaged()
    {
        base.DisposeManaged();
        Selections.Dispose();
    }

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();
        DescriptionHash?.Dispose();
    }
}
