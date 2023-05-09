using ElectionGuard.Ballot;
using ElectionGuard.Decryption.Accumulation;
using ElectionGuard.Guardians;

namespace ElectionGuard.Decryption.Challenge;

/// <summary>
/// 
/// </summary>
public class SelectionChallenge : DisposableBase
{
    /// <summary>
    /// The object id of the selection.
    /// </summary>
    public string ObjectId { get; init; }

    public string GuardianId { get; init; }

    // the sequence order of the guardian
    public ulong SequenceOrder { get; init; }

    public ElementModQ Coefficient { get; init; }

    // the challenge adjusted by the guardian lagrange coefficient
    // // ci =(c·wi)modq in the spec.
    public ElementModQ Challenge { get; init; }

    public const string HASH_PREFIX = "06";


    public SelectionChallenge(
        IElectionGuardian guardian,
        ElementModQ coefficient,
        ElementModP publicKey,
        ElementModQ extendedBaseHash,
        ICiphertextSelection ciphertext,
        AccumulatedSelection accumulation)
    {
        ObjectId = ciphertext.ObjectId;
        SequenceOrder = guardian.SequenceOrder;
        GuardianId = guardian.GuardianId;
        Coefficient = new(coefficient);
        using var challenge = ComputeChallenge(
            extendedBaseHash,
            publicKey,
            ciphertext.Ciphertext,
            accumulation.Commitment,
            accumulation.Value);

        // ci =(c·wi)modq
        Challenge = BigMath.MultModQ(challenge, coefficient);
    }

    public SelectionChallenge(
        string selectionId,
        IElectionGuardian guardian,
        ElementModQ coefficient,
        ElementModP publicKey,
        ElementModQ extendedBaseHash,
        ElGamalCiphertext ciphertext,
        ElGamalCiphertext commitment,
        ElementModP mBar)
    {
        ObjectId = selectionId;
        SequenceOrder = guardian.SequenceOrder;
        GuardianId = guardian.GuardianId;
        Coefficient = new(coefficient);
        using var challenge = ComputeChallenge(
            extendedBaseHash,
            publicKey,
            ciphertext,
            commitment,
            mBar);
        // ci =(c·wi)modq
        Challenge = BigMath.MultModQ(challenge, coefficient);
    }

    public SelectionChallenge(
        IElectionSelection selection,
        IElectionGuardian guardian,
        LagrangeCoefficient coefficient,
        ElementModQ challenge)
    {
        ObjectId = selection.ObjectId;
        GuardianId = guardian.GuardianId;
        SequenceOrder = guardian.SequenceOrder;
        Coefficient = new(coefficient.Coefficient);
        // ci =(c·wi)modq
        Challenge = BigMath.MultModQ(challenge, coefficient.Coefficient);
    }

    public SelectionChallenge(
        IElectionSelection selection,
        IElectionGuardian guardian,
        ElementModQ coefficient,
        ElementModQ challenge)
    {
        ObjectId = selection.ObjectId;
        GuardianId = guardian.GuardianId;
        SequenceOrder = guardian.SequenceOrder;
        Coefficient = new(coefficient);
        // ci =(c·wi)modq
        Challenge = BigMath.MultModQ(challenge, coefficient);
    }

    // // ci =(c·wi)modq
    [Obsolete("just for testing")]
    public SelectionChallenge(
        IElectionSelection selection,
        IElectionGuardian guardian,
        ElementModQ challenge)
    {
        ObjectId = selection.ObjectId;
        GuardianId = guardian.GuardianId;
        SequenceOrder = guardian.SequenceOrder;
        Challenge = new(challenge);
    }

    public SelectionChallenge(
        string selectionId,
        string guardianId,
        ulong sequenceOrder,
        ElementModQ coefficient,
        ElementModQ challenge)
    {
        ObjectId = selectionId;
        GuardianId = guardianId;
        SequenceOrder = sequenceOrder;
        Coefficient = new(coefficient);
        // ci =(c·wi)modq
        Challenge = BigMath.MultModQ(challenge, coefficient);
    }

    public SelectionChallenge(
        SelectionChallenge other)
    {
        ObjectId = other.ObjectId;
        GuardianId = other.GuardianId;
        SequenceOrder = other.SequenceOrder;
        Coefficient = new(other.Coefficient);
        Challenge = new(other.Challenge);
    }

    // c = H(06,Q;K,A,B,a,b,M)
    public static ElementModQ ComputeChallenge(
            CiphertextElectionContext context,
            ICiphertextSelection selection,
            AccumulatedSelection accumulated)
    {
        return ComputeChallenge(
                    context.CryptoExtendedBaseHash,
                    context.ElGamalPublicKey,
                    selection.Ciphertext,
                    accumulated.Commitment,
                    accumulated.Value);
    }

    // c = H(06,Q;K,A,B,a,b,M)
    public static ElementModQ ComputeChallenge(
        ElementModQ extendedHash,
        ElementModP elGamalPublicKey,
        ICiphertextSelection selection,
        AccumulatedSelection accumulated)
    {
        return ComputeChallenge(
                    extendedHash,
                    elGamalPublicKey,
                    selection.Ciphertext,
                    accumulated.Commitment,
                    accumulated.Value);
    }

    // c = H(06,Q;K,A,B,a,b,M)
    public static ElementModQ ComputeChallenge(
        ElementModQ extendedHash,
        ElementModP elGamalPublicKey,
        ElGamalCiphertext ciphertext,
        ElGamalCiphertext commitment,
        ElementModP mBar)
    {
        return Hash.HashElems(
                    HASH_PREFIX,
                    extendedHash,
                    elGamalPublicKey,
                    ciphertext.Pad,
                    ciphertext.Data,
                    commitment.Pad,
                    commitment.Data,
                    mBar);
    }

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();
        Challenge.Dispose();
    }
}
