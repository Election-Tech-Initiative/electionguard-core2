namespace ElectionGuard.Decryption.Tally;

public static partial class ContestDescriptionExtensions
{
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

        // placeholders
        foreach (var placeholder in contest.Placeholders)
        {
            selections.Add(
                placeholder.ObjectId,
                new CiphertextTallySelection(placeholder));
        }

        return selections;
    }
}

public class CiphertextTallyContest : DisposableBase, IEquatable<CiphertextTallyContest>
{
    public string ObjectId { get; init; } = default!;
    public ulong SequenceOrder { get; init; }
    public ElementModQ DescriptionHash { get; init; } = default!;
    public Dictionary<string, CiphertextTallySelection> Selections { get; init; } = default!;

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

    public CiphertextTallyContest(ContestDescriptionWithPlaceholders contestDescription)
    {
        ObjectId = contestDescription.ObjectId;
        SequenceOrder = contestDescription.SequenceOrder;
        DescriptionHash = contestDescription.CryptoHash();
        Selections = contestDescription.ToCiphertextTallySelectionDictionary();
    }

    public void Accumulate(CiphertextBallotContest contest)
    {
        foreach (var selection in contest.Selections)
        {
            _ = Selections[selection.ObjectId].Accumulate(selection);
        }
    }

    public async Task AccumulateAsync(CiphertextBallotContest contest)
    {
        var tasks = new List<Task>();
        foreach (var selection in contest.Selections)
        {
            tasks.Add(
                Selections[selection.ObjectId].AccumulateAsync(selection)
            );
        }
        await Task.WhenAll(tasks);
    }

    public void Accumulate(IEnumerable<CiphertextBallotContest> contests)
    {
        foreach (var contest in contests)
        {
            Accumulate(contest);
        }
    }

    public async Task AccumulateAsync(IEnumerable<CiphertextBallotContest> contests)
    {
        foreach (var contest in contests)
        {
            await AccumulateAsync(contest);
        }
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
