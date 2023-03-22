namespace ElectionGuard.Decryption.Tally;

public static partial class ContestDescriptionExtensions
{
    public static Dictionary<string, PlaintextTallySelection> ToPlaintextTallySelectionDictionary(
        this ContestDescription contestDescription)
    {
        var selections = new Dictionary<string, PlaintextTallySelection>();
        foreach (var selectionDescription in contestDescription.Selections)
        {
            selections.Add(
                selectionDescription.ObjectId,
                new PlaintextTallySelection(selectionDescription));
        }

        return selections;
    }

    public static Dictionary<string, PlaintextTallySelection> ToPlaintextTallySelectionDictionary(
        this ContestDescriptionWithPlaceholders contestDescription)
    {
        var selections = new Dictionary<string, PlaintextTallySelection>();
        foreach (var selectionDescription in contestDescription.Selections)
        {
            selections.Add(
                selectionDescription.ObjectId,
                new PlaintextTallySelection(selectionDescription));
        }

        // TODO: placeholders?

        return selections;
    }
}

public class PlaintextTallyContest
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
        ContestDescription contestDescription)
    {
        ObjectId = contestDescription.ObjectId;
        SequenceOrder = contestDescription.SequenceOrder;
        DescriptionHash = contestDescription.CryptoHash();
        Selections = contestDescription.ToPlaintextTallySelectionDictionary();
    }

    public PlaintextTallyContest(
        ContestDescriptionWithPlaceholders contestDescription)
    {
        ObjectId = contestDescription.ObjectId;
        SequenceOrder = contestDescription.SequenceOrder;
        DescriptionHash = contestDescription.CryptoHash();
        Selections = contestDescription.ToPlaintextTallySelectionDictionary();
    }

}
