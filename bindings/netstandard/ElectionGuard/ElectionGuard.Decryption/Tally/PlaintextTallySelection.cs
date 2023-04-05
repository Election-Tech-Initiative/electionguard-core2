using ElectionGuard.Encryption.Ballot;
namespace ElectionGuard.Decryption.Tally;

/// <summary>
/// A plaintext Tally Selection is a decrypted selection of a contest
/// </summary>
public class PlaintextTallySelection
    : DisposableBase, IElectionSelection, IEquatable<PlaintextTallySelection>
{
    public string ObjectId { get; init; }

    /// <summary>
    /// The sequence order of the selection
    /// </summary>
    public ulong SequenceOrder { get; init; }

    /// <summary>
    /// The hash of the SelectionDescription
    /// </summary>
    public ElementModQ DescriptionHash { get; init; }

    /// <summary>
    /// The decrypted representation of the sum of all ballots for the selection
    /// </summary>
    public ulong Tally { get; set; }

    /// <summary>
    /// The decrypted representation of the sum of all ballots for the selection
    /// g^tally or M in the spec
    /// </summary>
    public ElementModP Value { get; set; }

    public PlaintextTallySelection(
        IElectionSelection selection) : this(
            selection.ObjectId,
            selection.SequenceOrder,
            selection.DescriptionHash,
            0,
            Constants.ONE_MOD_P)
    {

    }

    public PlaintextTallySelection(
        string objectId,
        ulong sequenceOrder,
        ElementModQ descriptionHash) : this(
            objectId, sequenceOrder,
            descriptionHash,
            0,
            Constants.ONE_MOD_P)
    {

    }

    public PlaintextTallySelection(
        IElectionSelection selection,
        ulong tally,
        ElementModP value) : this(
            selection.ObjectId,
            selection.SequenceOrder,
            selection.DescriptionHash,
            tally, value)
    {

    }

    public PlaintextTallySelection(
        string objectId,
        ulong sequenceOrder,
        ElementModQ descriptionHash,
        ulong tally,
        ElementModP value)
    {
        ObjectId = objectId;
        SequenceOrder = sequenceOrder;
        DescriptionHash = descriptionHash;
        Tally = tally;
        Value = value;
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
               Value.Equals(other.Value);
        return equal;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as PlaintextTallySelection);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ObjectId, SequenceOrder, DescriptionHash.ToHex(), Tally, Value.ToHex());
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
