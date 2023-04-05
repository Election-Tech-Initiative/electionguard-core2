using ElectionGuard.ElectionSetup;

namespace ElectionGuard.Decryption.Decryption;

// a share of a guardian's decryption of a collection of ballots (usually all of the spoiled ballots)
public record class CiphertextDecryptionBallotShares : DisposableRecordBase, IEquatable<CiphertextDecryptionBallotShares>
{
    // key is guardian id
    public Dictionary<string, CiphertextDecryptionBallotShare> Shares { get; init; } = default!;

    public CiphertextDecryptionBallotShares(
        Dictionary<string, CiphertextDecryptionBallotShare> shares)
    {
        Shares = shares;
    }
}
