using System.Collections.Generic;

namespace ElectionGuard.ElectionSetup;

/// <summary>
/// A tuple of election public key and owner information
/// </summary>
public class ElectionPublicKey
{
    public ElectionPublicKey(
        string ownerId,
        int sequenceOrder,
        ElementModP publicKey,
        List<ElementModP> publicCommitments,
        List<SchnorrProof> coefficientProofs)
    {
        OwnerId = ownerId;
        SequenceOrder = sequenceOrder;
        Key = publicKey;
        CoefficientCommitments = publicCommitments;
        CoefficientProofs = coefficientProofs;
    }

    /// <summary>
    /// The id of the owner guardian
    /// </summary>
    public string OwnerId { get; }

    /// <summary>
    /// The sequence order of the owner guardian
    /// </summary>
    public int SequenceOrder { get; }

    /// <summary>
    /// The election public for the guardian
    /// Note: This is the same as the first coefficient commitment
    /// </summary>
    public ElementModP Key { get; }

    /// <summary>
    /// The commitments for the coefficients in the secret polynomial
    /// </summary>
    public List<ElementModP> CoefficientCommitments { get; }

    /// <summary>
    /// The proofs for the coefficients in the secret polynomial
    /// </summary>
    public List<SchnorrProof> CoefficientProofs { get; }
}
