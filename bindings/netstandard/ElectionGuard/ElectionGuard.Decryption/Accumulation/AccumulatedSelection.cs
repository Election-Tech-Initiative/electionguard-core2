using ElectionGuard.Decryption.Shares;
using ElectionGuard.Decryption.Extensions;
using ElectionGuard.Ballot;
using ElectionGuard.Guardians;
using ElectionGuard.ElectionSetup.Extensions;

namespace ElectionGuard.Decryption.Accumulation;

/// <summary>
/// accumulated collection of all guardian shares for a given selection.
/// This object is used to compute the final selection value.
///
/// This is a mutable class that is filled in through the decrypion process.
/// </summary>
public class AccumulatedSelection : DisposableBase, IElectionSelection
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
    /// The accumulated value of the selection. `M-bar` in the spec
    /// </summary>
    public ElementModP Value { get; private set; }

    /// <summary>
    /// The accumulated commitment of all guardian commitments.
    /// </summary>
    public ElGamalCiphertext Commitment { get; private set; }

    /// <summary>
    /// The proof that the accumulated value is correct.
    /// </summary>
    public ChaumPedersenProof? Proof { get; private set; }

    public Dictionary<string, SelectionShare> Shares { get; private set; } = new();

    /// <summary>
    /// Create an empty AccumulatedSelection object initialized with default values
    /// </summary>
    public AccumulatedSelection(
        IElectionSelection selection)
    {
        ObjectId = selection.ObjectId;
        SequenceOrder = selection.SequenceOrder;
        DescriptionHash = new(selection.DescriptionHash);
        Value = new(Constants.ONE_MOD_P);
        Commitment = new(Constants.ONE_MOD_P, Constants.ONE_MOD_P);
    }

    public AccumulatedSelection(
        AccumulatedSelection other)
    {
        ObjectId = other.ObjectId;
        SequenceOrder = other.SequenceOrder;
        DescriptionHash = new(other.DescriptionHash);
        Value = new(other.Value);
        Proof = new(other.Proof);
        Commitment = new(other.Commitment);
        Shares = other.Shares.ToDictionary(
            x => x.Key,
            x => new SelectionShare(x.Value));
    }

    /// <summary>
    /// Accumulate a share into the selection.
    /// </summary>
    public void Accumulate(
        List<Tuple<ElectionPublicKey, SelectionShare>> guardianShares,
        bool skipValidation = false)
    {
        if (!skipValidation)
        {
            // TODO: validation
        }

        var lagrangeCoefficients = guardianShares.ComputeLagrangeCoefficients();

        foreach (var (guardian, share) in guardianShares)
        {
            Accumulate(share, lagrangeCoefficients[guardian.GuardianId]);
        }
    }

    /// <summary>
    /// Accumulate a share into the selection.
    /// </summary>
    public void Accumulate(
        List<Tuple<ElectionPublicKey, SelectionShare>> guardianShares,
        Dictionary<string, LagrangeCoefficient> lagrangeCoefficients,
        bool skipValidation = false)
    {
        if (!skipValidation)
        {
            // TODO: validation
        }

        foreach (var (guardian, share) in guardianShares)
        {
            Accumulate(share, lagrangeCoefficients[guardian.GuardianId]);
        }
    }

    /// <summary>
    /// Accumulate a share into the selection.
    /// </summary>
    public void Accumulate(
        SelectionShare share,
        LagrangeCoefficient lagrangeCoefficient,
        bool skipValidation = false)
    {
        if (!skipValidation)
        {
            // TODO: validation
        }

        Shares.Add(share.GuardianId, share);
        Accumulate(
            share.Share,
            share.Commitment,
            lagrangeCoefficient.Coefficient);
    }

    public void AddProof(ChaumPedersenProof proof)
    {
        Proof = new(proof);
    }

    public bool IsValid()
    {
        // TODO: actual validation
        if (Shares.Count == 0)
        {
            return false;
        }

        if (Proof is null)
        {
            return false;
        }

        return true;
    }

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();
        Value.Dispose();
        DescriptionHash.Dispose();
        Proof?.Dispose();
        Shares.Dispose();
    }

    private void Accumulate(
        ElementModP share,
        ElGamalCiphertext commitment,
        ElementModQ lagrangeCoefficient)
    {
        // 𝑀𝑏𝑎𝑟 = 𝑀𝑏𝑎𝑟 * (𝑀𝑖 ^ 𝑤𝑖) mod p
        var interpolatedshare = share.PowModP(lagrangeCoefficient);
        Value = Value.MultModP(interpolatedshare);


        // TODO: double check this
        // a= Yai modp, b= Ybi modp.
        Commitment = Commitment.Add(commitment);
    }
}
