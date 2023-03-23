namespace ElectionGuard.Decryption.Tally;

/// <summary>
/// A plaintext Tally Selection is a decrypted selection of a contest
/// </summary>
public class PlaintextTallySelection : IEquatable<PlaintextTallySelection>
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

    // TODO: this is a placeholder for now
    public ulong[]? Proof { get; set; }

    public PlaintextTallySelection(
        SelectionDescription selection)
    {
        ObjectId = selection.ObjectId;
        SequenceOrder = selection.SequenceOrder;
        DescriptionHash = selection.CryptoHash();
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

    # region IEquatable

    public bool Equals(PlaintextTallySelection? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        var equal = ObjectId == other.ObjectId &&
               SequenceOrder == other.SequenceOrder &&
               DescriptionHash.Equals(other.DescriptionHash) &&
               Tally == other.Tally &&
               Value.Equals(other.Value); // &&
                                          // TODO: Proof.SequenceEqual(other.Proof); // just a placeholder for now
        return equal;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as PlaintextTallySelection);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ObjectId, SequenceOrder, DescriptionHash.ToHex(), Tally, Value.ToHex(), Proof);
    }

    public static bool operator ==(PlaintextTallySelection? left, PlaintextTallySelection? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(PlaintextTallySelection? left, PlaintextTallySelection? right)
    {
        return !Equals(left, right);
    }

    # endregion
}
