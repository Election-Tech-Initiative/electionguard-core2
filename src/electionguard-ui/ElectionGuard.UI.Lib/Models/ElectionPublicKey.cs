using ElectionGuard.Proofs;
using ElectionGuard.UI.Lib.Extensions;
using Newtonsoft.Json;

namespace ElectionGuard.UI.Lib.Models;

/// <summary>
/// A tuple of election public key and owner information
/// </summary>
public class ElectionPublicKey : DisposableBase
{
    [JsonConstructor]
    public ElectionPublicKey(
        string ownerId,
        ulong sequenceOrder,
        ElementModP key,
        List<ElementModP> coefficientCommitments,
        List<SchnorrProof> coefficientProofs)
    {
        OwnerId = ownerId;
        SequenceOrder = sequenceOrder;
        Key = new(key);
        CoefficientCommitments = coefficientCommitments
            .Select(x => new ElementModP(x)).ToList();
        CoefficientProofs = coefficientProofs
            .Select(x => new SchnorrProof(x)).ToList();
    }

    public ElectionPublicKey(ElectionPublicKey other)
    {
        OwnerId = other.OwnerId;
        SequenceOrder = other.SequenceOrder;
        Key = new(other.Key);
        CoefficientCommitments = other.CoefficientCommitments
            .Select(x => new ElementModP(x)).ToList();
        CoefficientProofs = other.CoefficientProofs
            .Select(x => new SchnorrProof(x)).ToList();
    }

    /// <summary>
    /// The id of the owner guardian
    /// </summary>
    public string OwnerId { get; init; }

    /// <summary>
    /// The sequence order of the owner guardian
    /// </summary>
    public ulong SequenceOrder { get; init; }

    /// <summary>
    /// The election public for the guardian
    /// Note: This is the same as the first coefficient commitment
    /// </summary>
    public ElementModP Key { get; init; }

    /// <summary>
    /// The commitments for the coefficients in the secret polynomial
    /// </summary>
    public List<ElementModP> CoefficientCommitments { get; init; }

    /// <summary>
    /// The proofs for the coefficients in the secret polynomial
    /// </summary>
    public List<SchnorrProof> CoefficientProofs { get; init; }

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();

        Key.Dispose();
        CoefficientCommitments.Dispose();
        CoefficientProofs.Dispose();
    }
}
