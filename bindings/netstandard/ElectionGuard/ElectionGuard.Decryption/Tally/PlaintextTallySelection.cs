namespace ElectionGuard.Decryption.Tally;

/// <summary>
/// A plaintext Tally Selection is a decrypted selection of a contest
/// </summary>
public class PlaintextTallySelection
{
    public string ObjectId { get; set; } = default!;

    /// <summary>
    /// The sequence order of the selection
    /// </summary>
    public ulong SequenceOrder { get; init; }

    /// <summary>
    /// The hash of the SelectionDescription
    /// </summary>
    public ElementModQ DescriptionHash { get; init; } = default!;

    /// <summary>
    /// The decrypted representation of the sum of all ballots for the selection
    /// </summary>
    public ulong Tally { get; set; }

    /// <summary>
    /// The decrypted representation of the sum of all ballots for the selection
    /// g^tally or M in the spec
    /// </summary>
    public ElementModP Value { get; set; }

    public ulong[]? Proof { get; set; }

    public PlaintextTallySelection(
        SelectionDescription selectionDescription)
    {
        ObjectId = selectionDescription.ObjectId;
        SequenceOrder = selectionDescription.SequenceOrder;
        DescriptionHash = selectionDescription.CryptoHash();
        Tally = 0;
        Value = Constants.ONE_MOD_P;
        Proof = null;
    }

    public PlaintextTallySelection(
        string objectId,
        ulong sequenceOrder,
        ElementModQ descriptionHash)
    {
        ObjectId = objectId;
        SequenceOrder = sequenceOrder;
        DescriptionHash = descriptionHash;
        Tally = 0;
        Value = Constants.ONE_MOD_P;
        Proof = null;
    }

    public PlaintextTallySelection(
        string objectId,
        ulong sequenceOrder,
        ElementModQ descriptionHash,
        ulong tally,
        ElementModP value,
        ulong[]? proof)
    {
        ObjectId = objectId;
        SequenceOrder = sequenceOrder;
        DescriptionHash = descriptionHash;
        Tally = tally;
        Value = value;
        Proof = proof;
    }
}
