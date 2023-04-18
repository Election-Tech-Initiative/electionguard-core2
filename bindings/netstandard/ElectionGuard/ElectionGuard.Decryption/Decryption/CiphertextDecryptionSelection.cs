using ElectionGuard.Encryption.Ballot;
using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.Decryption.Decryption;

/// <summary>
/// accumulated collection of all guardian shares for a given selection.
/// This object is used to compute the final selection value.
/// </summary>
public class CiphertextDecryptionSelection : DisposableBase, IElectionSelection
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
    /// The proof that the accumulated value is correct.
    /// </summary>
    public ChaumPedersenProof? Proof { get; private set; }

    // TODO: hashset
    public List<CiphertextDecryptionSelectionShare> Shares { get; private set; } = new();

    public CiphertextDecryptionSelection(
        string objectId,
        ulong sequenceOrder,
        ElementModQ descriptionHash)
    {
        ObjectId = objectId;
        SequenceOrder = sequenceOrder;
        DescriptionHash = descriptionHash;
        Value = Constants.ONE_MOD_P;
    }

    public CiphertextDecryptionSelection(
        ICiphertextSelection selection)
    {
        ObjectId = selection.ObjectId;
        SequenceOrder = selection.SequenceOrder;
        DescriptionHash = selection.DescriptionHash;
        Value = Constants.ONE_MOD_P;
    }

    /// <summary>
    /// Accumulate a share into the selection.
    /// </summary>
    public void Accumulate(
        List<Tuple<ElectionPublicKey, CiphertextDecryptionSelectionShare>> guardianShares,
        bool skipValidation = false)
    {
        if (!skipValidation)
        {
            // TODO: validation
        }

        foreach (var (guardian, share) in guardianShares)
        {
            var otherSequenceOrders = guardianShares
                .Where(i => i.Item1.OwnerId != guardian.OwnerId)
                .Select(x => x.Item1.SequenceOrder).ToList();
            var lagrangeCoefficient = Polynomial.Interpolate(
                guardian.SequenceOrder, otherSequenceOrders
                );
            Accumulate(share, lagrangeCoefficient);
        }
    }

    /// <summary>
    /// Accumulate a share into the selection.
    /// </summary>
    public void Accumulate(
        List<Tuple<ElectionPublicKey, CiphertextDecryptionSelectionShare>> guardianShares,
        Dictionary<string, ElementModQ> lagrangeCoefficients,
        bool skipValidation = false)
    {
        if (!skipValidation)
        {
            // TODO: validation
        }

        foreach (var (guardian, share) in guardianShares)
        {
            Accumulate(share, lagrangeCoefficients[guardian.OwnerId]);
        }
    }

    /// <summary>
    /// Accumulate a share into the selection.
    /// </summary>
    public void Accumulate(
        CiphertextDecryptionSelectionShare share,
        ElementModQ lagrangeCoefficient,
        bool skipValidation = false)
    {
        if (!skipValidation)
        {
            // TODO: validation
        }

        Shares.Add(share);
        Accumulate(share.Share, lagrangeCoefficient);
    }

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();
        Value.Dispose();
        DescriptionHash.Dispose();
    }

    private void Accumulate(
        ElementModP share,
        ElementModQ lagrangeCoefficient)
    {
        Console.WriteLine($"Accumulate: {share} {lagrangeCoefficient} {Value}");

        // M-bar = M-bar * (M_i ^ w_i) mod p
        // 𝑀𝑏𝑎𝑟 = 𝑀𝑏𝑎𝑟 * (𝑀𝑖 ^ 𝑤𝑖) mod p
        var interpolatedshare = share.PowModP(lagrangeCoefficient);
        Value = Value.MultModP(interpolatedshare);
    }
}
