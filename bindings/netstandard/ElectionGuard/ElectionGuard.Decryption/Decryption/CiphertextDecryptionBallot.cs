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
        if (!IsValid(share, guardianPublicKey))
        {
            throw new ArgumentException("Invalid share");
        }

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

    // check if the share is valid for submission
    public bool IsValid(
        CiphertextDecryptionBallotShare share,
        ElectionPublicKey guardianPublicKey)
    {
        if (share.BallotId != BallotId)
        {
            return false;
        }

        if (share.StyleId != StyleId)
        {
            return false;
        }

        if (!share.ManifestHash.Equals(ManifestHash))
        {
            return false;
        }

        return share.GuardianId == guardianPublicKey.OwnerId;
    }

    // check if this class is valid for the ballot
    public bool IsValid(
        CiphertextBallot ballot,
        ElementModQ extendedBaseHash)
    {
        if (ballot.ObjectId != BallotId)
        {
            return false;
        }

        if (ballot.StyleId != StyleId)
        {
            return false;
        }

        if (!ballot.ManifestHash.Equals(ManifestHash))
        {
            return false;
        }

        foreach (var share in Shares)
        {
            if (!share.Value.IsValid(ballot, GuardianPublicKeys[share.Key], extendedBaseHash))
            {
                return false;
            }
        }

        return true;
    }

    private void AddShare(CiphertextDecryptionBallotShare share)
    {
        if (!Shares.ContainsKey(share.GuardianId))
        {
            Shares.Add(share.GuardianId, share);
        }
    }
}
