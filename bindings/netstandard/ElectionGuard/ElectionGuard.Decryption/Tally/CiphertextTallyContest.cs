namespace ElectionGuard.Decryption.Tally;

public static partial class ContestDescriptionExtensions
{
    public static Dictionary<string, CiphertextTallySelection> ToCiphertextTallySelectionDictionary(
        this ContestDescription contestDescription)
    {
        var selections = new Dictionary<string, CiphertextTallySelection>();
        foreach (var selectionDescription in contestDescription.Selections)
        {
            selections.Add(
                selectionDescription.ObjectId,
                new CiphertextTallySelection(selectionDescription));
        }

        return selections;
    }

    public static Dictionary<string, CiphertextTallySelection> ToCiphertextTallySelectionDictionary(
        this ContestDescriptionWithPlaceholders contestDescription)
    {
        var selections = new Dictionary<string, CiphertextTallySelection>();
        foreach (var selectionDescription in contestDescription.Selections)
        {
            selections.Add(
                selectionDescription.ObjectId,
                new CiphertextTallySelection(selectionDescription));
        }

        // TODO: placeholders?

        return selections;
    }
}

public class CiphertextTallyContest : DisposableBase
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

    public CiphertextTallyContest(ContestDescription contestDescription)
    {
        ObjectId = contestDescription.ObjectId;
        SequenceOrder = contestDescription.SequenceOrder;
        DescriptionHash = contestDescription.CryptoHash();
        Selections = contestDescription.ToCiphertextTallySelectionDictionary();
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
}
