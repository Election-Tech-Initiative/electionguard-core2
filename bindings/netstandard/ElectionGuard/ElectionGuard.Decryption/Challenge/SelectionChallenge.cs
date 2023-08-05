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
    /// 
    /// 𝑐𝑖 = (𝑐 • ω𝑖) mod q in the spec. Equation (61)
    /// </summary>
    public ElementModQ Challenge { get; init; }

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

        // 𝑐𝑖 = (𝑐 • ω𝑖) mod q.
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
        // 𝑐𝑖 = (𝑐 • ω𝑖) mod q
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
        // 𝑐𝑖 = (𝑐 • ω𝑖) mod q
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
        // 𝑐𝑖 = (𝑐 • ω𝑖) mod q
        Challenge = BigMath.MultModQ(challenge, coefficient);
    }

    [JsonConstructor]
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
        // 𝑐𝑖 = (𝑐 • ω𝑖) mod q
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
    /// compute a challenge value by hashing the inputs together. 
    /// The challenge is computed as follows:
    /// c = H(HE;30,K,A,B,a,b,M). Ballot Selection Decryption Proof 3.6.3
    /// c = H(HE;30,K,α,β,a,b,M). Challenge Ballot Selection Decryption Proof 3.6.5
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
    /// The challenge is computed as follows:
    /// c = H(HE;30,K,A,B,a,b,M). Ballot Selection Decryption Proof 3.6.3
    /// c = H(HE;30,K,α,β,a,b,M). Challenge Ballot Selection Decryption Proof 3.6.5
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
    /// The challenge is computed as follows:
    /// c = H(HE;30,K,A,B,a,b,M). Ballot Selection Decryption Proof 3.6.3
    /// c = H(HE;30,K,α,β,a,b,M). Challenge Ballot Selection Decryption Proof 3.6.5
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
                    Hash.Prefix_DecryptSelectionProof,
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
