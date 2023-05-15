using ElectionGuard.Ballot;
using ElectionGuard.Decryption.Extensions;
using ElectionGuard.Guardians;

namespace ElectionGuard.Decryption.Shares;

/// <summary>
/// A Guardian's Partial Decryption of a selection.
/// </summary>
public class SelectionShare
    : DisposableBase, IElectionSelection, IEquatable<SelectionShare>
{
    /// <summary>
    /// The object id of the selection.
    /// </summary>
    public string ObjectId { get; init; }

    /// <summary>
    /// The sequence order of the selection.
    /// </summary>
    public ulong SequenceOrder { get; init; }

    /// <summary>
    /// The hash of the SelectionDescription.
    /// </summary>
    public ElementModQ DescriptionHash { get; init; }

    /// <summary>
    /// The Guardian's Id.
    /// </summary>
    public string GuardianId { get; init; }

    /// <summary>
    /// The Guardian's share of the partial decryption. 
    /// `M_i` in the spec
    /// </summary>
    public ElementModP Share { get; init; }

    // commitment for generating the cp proof as part of decryption
    public ElGamalCiphertext Commitment { get; set; } = default!;

    public SelectionShare(
        string objectId,
        ulong sequenceOrder,
        ElementModQ descriptionHash,
        string guardianId,
        ElementModP share,
        ElGamalCiphertext commitment)
    {
        ObjectId = objectId;
        SequenceOrder = sequenceOrder;
        DescriptionHash = new(descriptionHash);
        GuardianId = guardianId;
        Share = new(share);
        Commitment = new(commitment);
    }

    public SelectionShare(
        ICiphertextSelection selection,
        string guardianId,
        ElementModP share,
        ElGamalCiphertext commitment)
    {
        ObjectId = selection.ObjectId;
        SequenceOrder = selection.SequenceOrder;
        DescriptionHash = new(selection.DescriptionHash);
        GuardianId = guardianId;
        Share = new(share);
        Commitment = new(commitment);
    }

    public SelectionShare(
        SelectionShare other)
    {
        ObjectId = other.ObjectId;
        SequenceOrder = other.SequenceOrder;
        DescriptionHash = new(other.DescriptionHash);
        GuardianId = other.GuardianId;
        Share = new(other.Share);
        Commitment = new(other.Commitment);
    }

    /// <summary>
    /// Verify that this CiphertextDecryptionSelection is valid for a
    /// specific guardian, and extended base hash. We allow any ICiphertextSelection
    /// to be passed in, so that we can use this method to verify either
    /// a ballot or a tally.
    /// </summary>
    public bool IsValid(
        ICiphertextSelection message,
        ElectionPublicKey guardian)
    {
        if (guardian.GuardianId != GuardianId)
        {
            return false;
        }

        if (message.ObjectId != ObjectId)
        {
            return false;
        }

        if (message.DescriptionHash is null
            || !message.DescriptionHash.Equals(DescriptionHash))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Verify that this share is a valid decryption share for a 
    /// specific ciphertext, challenge, commitment offset, and response.
    /// </summary>
    public bool IsValidShare(
        ElGamalCiphertext message,
        ElementModQ challenge,
        ElementModP commitmentOffset,
        ElementModQ response)
    {
        using var recomputedCommitment = this.ComputeCommitment(
            message.Pad,
            challenge,
            commitmentOffset,
            response);

        return recomputedCommitment.Equals(Commitment);
    }

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();
        DescriptionHash?.Dispose();
        Share?.Dispose();
        Commitment?.Dispose();
    }

    #region Equality

    public bool Equals(SelectionShare? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return ObjectId == other.ObjectId &&
               SequenceOrder == other.SequenceOrder &&
               DescriptionHash.Equals(other.DescriptionHash) &&
               Share.Equals(other.Share) &&
               Commitment.Equals(other.Commitment);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || (obj is SelectionShare other && Equals(other));
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ObjectId, SequenceOrder, DescriptionHash, Share);
    }

    public static bool operator ==(SelectionShare? left, SelectionShare? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(SelectionShare? left, SelectionShare? right)
    {
        return !Equals(left, right);
    }

    #endregion
}
