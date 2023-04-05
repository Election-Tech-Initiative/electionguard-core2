using ElectionGuard.Encryption.Ballot;
using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.Decryption.Decryption;

// accumulated collection of all guardian shares for a given selection
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

    // M-bar in the spec
    public ElementModP Value { get; private set; }

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

    // TODO: this tuple is awkward
    public void Accumulate(
        List<Tuple<ElectionPublicKey, CiphertextDecryptionSelectionShare>> guardianShares,
        bool skipValidation = false)
    {
        // TODO: validation

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

    public void Accumulate(
        List<Tuple<ElectionPublicKey, CiphertextDecryptionSelectionShare>> guardianShares,
        Dictionary<string, ElementModQ> lagrangeCoefficients,
        bool skipValidation = false)
    {
        // TODO: validation

        foreach (var (guardian, share) in guardianShares)
        {
            Accumulate(share, lagrangeCoefficients[guardian.OwnerId]);
        }
    }

    public void Accumulate(
        Dictionary<string, CiphertextDecryptionSelectionShare> shares,
        Dictionary<string, ElementModQ> lagrangeCoefficients,
        bool skipValidation = false)
    {
        foreach (var (guardianId, share) in shares)
        {
            Accumulate(share, lagrangeCoefficients[guardianId]);
        }

    }

    public void Accumulate(
        List<CiphertextDecryptionSelectionShare> shares,
        List<ElementModQ> lagrangeCoefficients)
    {
        for (var i = 0; i < shares.Count; i++)
        {
            Accumulate(shares[i], lagrangeCoefficients[i]);
        }
    }

    public void Accumulate(
        CiphertextDecryptionSelectionShare share,
        ElementModQ lagrangeCoefficient)
    {
        Shares.Add(share);
        Accumulate(share.Share, lagrangeCoefficient);
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
