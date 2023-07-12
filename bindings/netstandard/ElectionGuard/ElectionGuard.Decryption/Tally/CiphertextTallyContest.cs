using ElectionGuard.Ballot;
using ElectionGuard.ElectionSetup.Extensions;

namespace ElectionGuard.Decryption.Tally;

/// <summary>
/// A CiphertextTallyContest is a container for associating a collection 
/// of CiphertextTallySelection to a specific ContestDescription
/// </summary>
public class CiphertextTallyContest : DisposableBase, ICiphertextContest, IEquatable<CiphertextTallyContest>
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
    /// The collection of selections using the object id as the key
    /// </summary>
    public Dictionary<string, CiphertextTallySelection> Selections { get; init; }

    IReadOnlyList<ICiphertextSelection> ICiphertextContest.Selections => Selections.Values.ToList();

    [JsonConstructor]
    public CiphertextTallyContest(
        string objectId, ulong sequenceOrder, ElementModQ descriptionHash,
        Dictionary<string, CiphertextTallySelection> selections)
    {
        ObjectId = objectId;
        SequenceOrder = sequenceOrder;
        DescriptionHash = descriptionHash;
        Selections = selections;
    }

    public CiphertextTallyContest(ContestDescription contest)
    {
        ObjectId = contest.ObjectId;
        SequenceOrder = contest.SequenceOrder;
        DescriptionHash = contest.CryptoHash();
        Selections = contest.ToCiphertextTallySelectionDictionary();
    }

    public CiphertextTallyContest(ContestDescriptionWithPlaceholders contest)
    {
        ObjectId = contest.ObjectId;
        SequenceOrder = contest.SequenceOrder;
        DescriptionHash = contest.CryptoHash();
        Selections = contest.ToCiphertextTallySelectionDictionary();
    }

    public CiphertextTallyContest(CiphertextTallyContest other)
    {
        ObjectId = other.ObjectId;
        SequenceOrder = other.SequenceOrder;
        DescriptionHash = new(other.DescriptionHash);
        Selections = other.Selections.Select(x => new CiphertextTallySelection(x.Value)).ToDictionary(x => x.ObjectId);
    }

    /// <summary>
    /// Accumulate a CiphertextBallotContest into the CiphertextTallyContest
    /// </summary>
    public void Accumulate(CiphertextBallotContest contest)
    {
        // check that the contest selections are 
        // all included in the selections collection
        // and ignore any placeholders
        var ballotSelections = contest.Selections
            .Where(i => !i.IsPlaceholder).ToList();

        var contestSelectionIds = ballotSelections
            .Select(i => i.ObjectId).ToList();

        if (!Selections.Keys.All(contestSelectionIds.Contains))
        {
            throw new ArgumentException("Selections do not match contest");
        }
        foreach (var selection in ballotSelections)
        {
            _ = Selections[selection.ObjectId].Accumulate(selection);
        }
    }

    public void Accumulate(CiphertextTallyContest contest)
    {
        if (!Selections.Keys.All(contest.Selections.Keys.Contains))
        {
            throw new ArgumentException("Selections do not match contest");
        }
        foreach (var (selectionId, selection) in contest.Selections)
        {
            _ = Selections[selectionId].Accumulate(selection);
        }
    }

    /// <summary>
    /// Accumulate a CiphertextBallotContest into the CiphertextTallyContest
    /// </summary>
    public async Task AccumulateAsync(
        CiphertextBallotContest contest,
        CancellationToken cancellationToken = default)
    {
        // check that the contest selections are 
        // all included in the selections collection
        // and ignore any placeholders
        var ballotSelections = contest.Selections
            .Where(i => !i.IsPlaceholder).ToList();

        var contestSelectionIds = ballotSelections
            .Select(i => i.ObjectId).ToList();

        if (!Selections.Keys.All(contestSelectionIds.Contains))
        {
            throw new ArgumentException("Selections do not match contest");
        }

        var tasks = new List<Task>();
        foreach (var selection in ballotSelections)
        {
            tasks.Add(
                Selections[selection.ObjectId].AccumulateAsync(
                    selection, cancellationToken)
            );
        }
        await Task.WhenAll(tasks);
    }

    public async Task AccumulateAsync(CiphertextTallyContest contest,
        CancellationToken cancellationToken = default)
    {
        if (!Selections.Keys.All(contest.Selections.Keys.Contains))
        {
            throw new ArgumentException("Selections do not match contest");
        }

        var tasks = new List<Task>();
        foreach (var (selectionId, selection) in contest.Selections)
        {
            tasks.Add(
                Selections[selectionId].AccumulateAsync(
                    selection, cancellationToken)
            );
        }
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Accumulate a collection of CiphertextBallotContest into the CiphertextTallyContest
    /// </summary>
    public void Accumulate(IEnumerable<CiphertextBallotContest> contests)
    {
        foreach (var contest in contests)
        {
            Accumulate(contest);
        }
    }

    /// <summary>
    /// Accumulate a collection of CiphertextBallotContest into the CiphertextTallyContest
    /// </summary>
    public async Task AccumulateAsync(
        IEnumerable<CiphertextBallotContest> contests,
        CancellationToken cancellationToken = default)
    {
        foreach (var contest in contests)
        {
            await AccumulateAsync(contest, cancellationToken);
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

    #region IEquatable

    public bool Equals(CiphertextTallyContest? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return ObjectId == other.ObjectId && SequenceOrder == other.SequenceOrder &&
               DescriptionHash.Equals(other.DescriptionHash) &&
               Selections.SequenceEqual(other.Selections);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || (obj is CiphertextTallyContest other && Equals(other));
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ObjectId, SequenceOrder, DescriptionHash, Selections);
    }

    public static bool operator ==(CiphertextTallyContest? left, CiphertextTallyContest? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(CiphertextTallyContest? left, CiphertextTallyContest? right)
    {
        return !Equals(left, right);
    }

    #endregion
}

public static partial class ContestDescriptionExtensions
{
    /// <summary>
    /// Converts a <see cref="ContestDescription"/> to a <see cref="CiphertextTallyContest"/>
    /// </summary>
    public static Dictionary<string, CiphertextTallySelection> ToCiphertextTallySelectionDictionary(
        this ContestDescription contest)
    {
        var selections = new Dictionary<string, CiphertextTallySelection>();
        foreach (var selectionDescription in contest.Selections)
        {
            selections.Add(
                selectionDescription.ObjectId,
                new CiphertextTallySelection(selectionDescription));
        }

        return selections;
    }

    /// <summary>
    /// Converts a <see cref="ContestDescriptionWithPlaceholders"/> to a <see cref="CiphertextTallyContest"/>
    /// </summary>
    public static Dictionary<string, CiphertextTallySelection> ToCiphertextTallySelectionDictionary(
        this ContestDescriptionWithPlaceholders contest)
    {
        var selections = new Dictionary<string, CiphertextTallySelection>();
        foreach (var selection in contest.Selections)
        {
            selections.Add(
                selection.ObjectId,
                new CiphertextTallySelection(selection));
        }

        // Do not add placeholoders to the tally

        return selections;
    }
}
