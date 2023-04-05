using ElectionGuard.ElectionSetup;
using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.Decryption.Decryption;

// a share of several guardian's decryption of a ballot (usually a spoiled ballot)
public record class CiphertextDecryptionBallot : DisposableRecordBase, IEquatable<CiphertextDecryptionBallot>
{
    public string BallotId { get; init; }
    public string StyleId { get; init; }
    public ElementModQ ManifestHash { get; init; }

    // key is guardian id
    public Dictionary<string, CiphertextDecryptionBallotShare> Shares { get; init; } = new();

    // key is guardian id
    public Dictionary<string, ElectionPublicKey> GuardianPublicKeys { get; init; } = new();

    public CiphertextDecryptionBallot(
        string ballotId,
        string styleId,
        ElementModQ manifestHash
    )
    {
        BallotId = ballotId;
        StyleId = styleId;
        ManifestHash = manifestHash;
    }

    public CiphertextDecryptionBallot(
        CiphertextDecryptionBallotShare share, ElectionPublicKey guardianPublicKey)
    {
        BallotId = share.BallotId;
        StyleId = share.StyleId;
        ManifestHash = share.ManifestHash;
        AddShare(share, guardianPublicKey);
    }

    public CiphertextDecryptionBallot(
        Dictionary<string, CiphertextDecryptionBallotShare> shares,
        Dictionary<string, ElectionPublicKey> guardianPublicKeys)
    {
        BallotId = shares.First().Value.BallotId;
        StyleId = shares.First().Value.StyleId;
        ManifestHash = shares.First().Value.ManifestHash;
        Shares = shares;
        GuardianPublicKeys = guardianPublicKeys;
    }

    public void AddShare(CiphertextDecryptionBallotShare share, ElectionPublicKey guardianPublicKey)
    {
        // TODO: validate that the share is for the same ballot

        if (!GuardianPublicKeys.ContainsKey(guardianPublicKey.OwnerId))
        {
            GuardianPublicKeys.Add(guardianPublicKey.OwnerId, guardianPublicKey);
        }

        AddShare(share);
    }

    public List<Tuple<ElectionPublicKey, CiphertextDecryptionBallotShare>> GetShares()
    {
        return Shares.Select(x => new Tuple<ElectionPublicKey, CiphertextDecryptionBallotShare>(
            GuardianPublicKeys[x.Key],
            x.Value
        )).ToList();
    }

    public Tuple<ElectionPublicKey, CiphertextDecryptionBallotShare> GetShare(string guardianId)
    {
        return new Tuple<ElectionPublicKey, CiphertextDecryptionBallotShare>(
            GuardianPublicKeys[guardianId],
            Shares[guardianId]
        );
    }

    private void AddShare(CiphertextDecryptionBallotShare share)
    {
        if (!Shares.ContainsKey(share.GuardianId))
        {
            Shares.Add(share.GuardianId, share);
        }
    }
}
