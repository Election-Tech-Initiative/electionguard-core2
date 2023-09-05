using System.Text;
using ElectionGuard.Ballot;

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
    /// t in the spec
    /// </summary>
    public ulong Tally { get; private set; }

    /// <summary>
    /// The decrypted representation of the sum of all ballots for the selection
    /// K^tally or T in the spec
    /// </summary>
    public ElementModP Value { get; private set; }

    /// <summary>
    /// The proof that the decrypted representation of the sum of all ballots for the selection is correct
    /// </summary>
    public ChaumPedersenProof? Proof { get; private set; }

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
        ElementModP decryptedValue,
        ChaumPedersenProof proof) : this(
            selection.ObjectId,
            selection.SequenceOrder,
            selection.DescriptionHash,
            tally, decryptedValue, proof)
    {
    }

    [JsonConstructor]
    public PlaintextTallySelection(
        string objectId,
        ulong sequenceOrder,
        ElementModQ descriptionHash,
        ulong tally,
        ElementModP value,
        ChaumPedersenProof? proof = null)
    {
        ObjectId = objectId;
        SequenceOrder = sequenceOrder;
        DescriptionHash = new(descriptionHash);
        Tally = tally;
        Value = new(value);
        if (proof is not null)
        {
            Proof = new(proof);
        }
    }

    public PlaintextTallySelection(
        PlaintextTallySelection other) : this(
            other.ObjectId,
            other.SequenceOrder,
            other.DescriptionHash,
            other.Tally,
            other.Value,
            other.Proof)
    {

    }

    public void Update(PlaintextTallySelection other)
    {
        Update(other.Tally, other.Value, other.Proof);
    }

    public void Update(ulong tally,
        ElementModP decryptedValue,
        ChaumPedersenProof? proof)
    {
        Tally = tally;
        Value = new(decryptedValue);
        if (proof != null)
        {
            Proof = new(proof);
        }
    }

    protected override void DisposeUnmanaged()
    {
        base.DisposeManaged();
        DescriptionHash?.Dispose();
        Value?.Dispose();
        Proof?.Dispose();
    }

    public override string ToString()
    {
        var builder = new StringBuilder();
        _ = builder.AppendLine($"Selection: {ObjectId} ({SequenceOrder}) {DescriptionHash}");
        _ = builder.AppendLine($"     Vote: {Tally}");
        _ = builder.AppendLine($"    Value: {Value}");
        return builder.ToString();
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
