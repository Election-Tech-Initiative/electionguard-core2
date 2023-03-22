namespace ElectionGuard.Decryption.Tally;

public static partial class ContestDescriptionExtensions
{
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

        // placeholders
        foreach (var placeholder in contest.Placeholders)
        {
            selections.Add(
                placeholder.ObjectId,
                new PlaintextTallySelection(placeholder));
        }

        return selections;
    }
}

public class PlaintextTallyContest : IEquatable<PlaintextTallyContest>
{
    public string ObjectId { get; init; } = default!;
    public ulong SequenceOrder { get; init; }
    public ElementModQ DescriptionHash { get; init; } = default!;
    public Dictionary<string, PlaintextTallySelection> Selections { get; init; } = default!;

    public PlaintextTallyContest(
        string objectId, ulong sequenceOrder, ElementModQ descriptionHash,
        Dictionary<string, PlaintextTallySelection> selections)
    {
        ObjectId = objectId;
        SequenceOrder = sequenceOrder;
        DescriptionHash = descriptionHash;
        Selections = selections;
    }

    public PlaintextTallyContest(
        ContestDescription contest)
    {
        ObjectId = contest.ObjectId;
        SequenceOrder = contest.SequenceOrder;
        DescriptionHash = contest.CryptoHash();
        Selections = contest.ToPlaintextTallySelectionDictionary();
    }

    public PlaintextTallyContest(
        ContestDescriptionWithPlaceholders contest)
    {
        ObjectId = contest.ObjectId;
        SequenceOrder = contest.SequenceOrder;
        DescriptionHash = contest.CryptoHash();
        Selections = contest.ToPlaintextTallySelectionDictionary();
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
        return ReferenceEquals(this, obj) || obj is PlaintextTallyContest other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ObjectId, SequenceOrder, DescriptionHash, Selections);
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
