using ElectionGuard.Ballot;
using ElectionGuard.Decryption.Accumulation;
using ElectionGuard.ElectionSetup;
using ElectionGuard.Guardians;

namespace ElectionGuard.Decryption.Challenge;

/// <summary>
/// A challenge for a specific guardian to prove that a selection share was generated correctly.
/// </summary>
public record SelectionChallenge : DisposableRecordBase
{
    /// <summary>
    /// The object id of the selection.
    /// </summary>
    public string ObjectId { get; init; }

    /// <summary>
    /// the guardian id for which this challenge belongs
    /// </summary>
    public string GuardianId { get; init; }

    // the sequence order of the guardian
    public ulong SequenceOrder { get; init; }

    /// <summary>
    /// The lagrange coefficient for the guardian
    /// </summary>
    public ElementModQ Coefficient { get; init; }

    /// <summary>
    /// The selection challenge adjusted by the guardian lagrange coefficient
    /// ci =(c·wi)modq in the spec.
    /// </summary>
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

        // ci =(c·wi) mod q
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

    /// <summary>
    /// constructor that accepts a pregenerated challenge
    ///
    /// This constructor is only for testing and should not be used in production
    /// </summary>
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
        SelectionChallenge other) : base(other)
    {
        ObjectId = other.ObjectId;
        GuardianId = other.GuardianId;
        SequenceOrder = other.SequenceOrder;
        Coefficient = new(other.Coefficient);
        Challenge = new(other.Challenge);
    }

    /// <summary>
    /// Given a selection and the proposed accumulated selection consisting 
    /// of all selection decryption shares submitted by all participating guardians, 
    /// compute a challenge value by hashing the inputs together. The challenge
    /// is computed as follows:
    /// c = H(06,Q;K,A,B,a,b,M)
    ///
    /// Once the challenge is computed, it is offset for each guardian by the lagrange coefficient
    /// and the result is stored in the SelectionChallenge object.
    /// </summary>
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

    /// <summary>
    /// Given a selection and the proposed accumulated selection consisting 
    /// of all selection decryption shares submitted by all participating guardians, 
    /// compute a challenge value by hashing the inputs together. The challenge
    /// is computed as follows:
    /// c = H(06,Q;K,A,B,a,b,M)
    ///
    /// Once the challenge is computed, it is offset for each guardian by the lagrange coefficient
    /// and the result is stored in the SelectionChallenge object.
    /// </summary>
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

    /// <summary>
    /// Given a selection and the proposed accumulated selection consisting 
    /// of all selection decryption shares submitted by all participating guardians, 
    /// compute a challenge value by hashing the inputs together. The challenge
    /// is computed as follows:
    /// c = H(06,Q;K,A,B,a,b,M)
    ///
    /// Once the challenge is computed, it is offset for each guardian by the lagrange coefficient
    /// and the result is stored in the SelectionChallenge object.
    /// </summary>
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
        Coefficient?.Dispose();
        Challenge?.Dispose();
    }
}
