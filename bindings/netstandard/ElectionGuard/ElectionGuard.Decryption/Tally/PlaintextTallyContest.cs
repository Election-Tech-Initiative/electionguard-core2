using ElectionGuard.Ballot;
using ElectionGuard.ElectionSetup.Extensions;

namespace ElectionGuard.Decryption.Tally;

/// <summary>
/// A plaintext Tally Contest is a collection of plaintext selections
/// </summary>
public class PlaintextTallyContest : DisposableBase, IElectionContest, IEquatable<PlaintextTallyContest>
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
    public Dictionary<string, PlaintextTallySelection> Selections { get; init; }

    [JsonConstructor]
    public PlaintextTallyContest(
        string objectId, ulong sequenceOrder, ElementModQ descriptionHash,
        Dictionary<string, PlaintextTallySelection> selections)
    {
        ObjectId = objectId;
        SequenceOrder = sequenceOrder;
        DescriptionHash = new(descriptionHash);
        Selections = selections
            .ToDictionary(x => x.Key, x => new PlaintextTallySelection(x.Value));
    }

    public PlaintextTallyContest(
        CiphertextBallotContest contest)
    {
        ObjectId = contest.ObjectId;
        SequenceOrder = contest.SequenceOrder;
        DescriptionHash = new(contest.DescriptionHash);
        Selections = contest.ToPlaintextTallySelectionDictionary();
    }

    public PlaintextTallyContest(
        ContestDescription contest)
    {
        ObjectId = contest.ObjectId;
        SequenceOrder = contest.SequenceOrder;
        DescriptionHash = new(contest.CryptoHash());
        Selections = contest.ToPlaintextTallySelectionDictionary();
    }

    public PlaintextTallyContest(
        ContestDescriptionWithPlaceholders contest)
    {
        ObjectId = contest.ObjectId;
        SequenceOrder = contest.SequenceOrder;
        DescriptionHash = new(contest.CryptoHash());
        Selections = contest.ToPlaintextTallySelectionDictionary();
    }

    public PlaintextTallyContest(PlaintextTallyContest other)
    {
        ObjectId = other.ObjectId;
        SequenceOrder = other.SequenceOrder;
        DescriptionHash = new(other.DescriptionHash);
        Selections = other.Selections
            .ToDictionary(x => x.Key, x => new PlaintextTallySelection(x.Value));
    }

    protected override void DisposeManaged()
    {
        base.DisposeManaged();
        Selections?.Dispose();
    }

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();
        DescriptionHash?.Dispose();
    }

    #region IEquatable

    public bool Equals(PlaintextTallyContest? other)
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
               Equals(DescriptionHash, other.DescriptionHash) &&
               Selections.OrderBy(x => x.Key).SequenceEqual(other.Selections.OrderBy(x => x.Key));
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || (obj is PlaintextTallyContest other && Equals(other));
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ObjectId, SequenceOrder, DescriptionHash, Selections.OrderBy(x => x.Key));
    }

    public static bool operator ==(PlaintextTallyContest? left, PlaintextTallyContest? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(PlaintextTallyContest? left, PlaintextTallyContest? right)
    {
        return !Equals(left, right);
    }

    #endregion
}

public static partial class ContestDescriptionExtensions
{
    /// <summary>
    /// Converts a <see cref="ContestDescription"/> to a dictionary of <see cref="PlaintextTallySelection"/>
    /// </summary>
    public static Dictionary<string, PlaintextTallySelection> ToPlaintextTallySelectionDictionary(
        this ContestDescription contest)
    {
        var selections = new Dictionary<string, PlaintextTallySelection>();
        foreach (var selection in contest.Selections)
        {
            selections.Add(
                selection.ObjectId,
                new PlaintextTallySelection(selection));
        }

        return selections;
    }

    /// <summary>
    /// Converts a <see cref="ContestDescriptionWithPlaceholders"/> to a dictionary of <see cref="PlaintextTallySelection"/>
    /// </summary>
    public static Dictionary<string, PlaintextTallySelection> ToPlaintextTallySelectionDictionary(
        this ContestDescriptionWithPlaceholders contest)
    {
        var selections = new Dictionary<string, PlaintextTallySelection>();
        foreach (var selection in contest.Selections)
        {
            selections.Add(
                selection.ObjectId,
                new PlaintextTallySelection(selection));
        }

        // Do not add placeholders

        return selections;
    }

    /// <summary>
    /// Converts a <see cref="ContestDescriptionWithPlaceholders"/> to a dictionary of <see cref="PlaintextTallySelection"/>
    /// </summary>
    public static Dictionary<string, PlaintextTallySelection> ToPlaintextTallySelectionDictionary(
        this CiphertextBallotContest contest)
    {
        var selections = new Dictionary<string, PlaintextTallySelection>();
        foreach (var selection in contest.Selections.Where(x => x.IsPlaceholder == false))
        {
            selections.Add(
                selection.ObjectId,
                new PlaintextTallySelection(selection));
        }

        // Do not add placeholders

        return selections;
    }
}
