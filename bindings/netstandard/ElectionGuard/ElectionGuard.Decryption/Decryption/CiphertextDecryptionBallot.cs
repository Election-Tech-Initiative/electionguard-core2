using ElectionGuard.Decryption.Shares;
using ElectionGuard.ElectionSetup;
using ElectionGuard.ElectionSetup.Extensions;
using ElectionGuard.Guardians;

namespace ElectionGuard.Decryption.Decryption;

/// <summary>
/// A representation of the dectyptin of a ballot. This object is used by the Decryption Mediator
/// to coordinate the decryption of a ballot.
/// </summary>
public record class CiphertextDecryptionBallot : DisposableRecordBase, IEquatable<CiphertextDecryptionBallot>
{
    public string BallotId { get; init; }
    public string StyleId { get; init; }
    public ElementModQ ManifestHash { get; init; }

    /// <summary>
    /// A collection of partial decryptions submitted by guardians
    /// key is guardian id
    /// </summary>
    public Dictionary<string, BallotShare> Shares { get; init; } = new();

    /// <summary>
    /// A collection of public keys for guardians that have submitted shares
    /// key is guardian id
    /// </summary>

    // TODO: make this a constructed member and not a serialized member?
    public Dictionary<string, ElectionPublicKey> GuardianPublicKeys { get; init; } = new();

    /// <summary>
    /// Create a new instance of a CiphertextDecryptionBallot
    /// </summary>
    public CiphertextDecryptionBallot(
        string ballotId,
        string styleId,
        ElementModQ manifestHash
    )
    {
        BallotId = ballotId;
        StyleId = styleId;
        ManifestHash = new(manifestHash);
    }

    /// <summary>
    /// Create a new instance of a CiphertextDecryptionBallot begining with a single share
    /// </summary>
    public CiphertextDecryptionBallot(
        BallotShare share,
        ElectionPublicKey guardianPublicKey)
    {
        BallotId = share.BallotId;
        StyleId = share.StyleId;
        ManifestHash = new(share.ManifestHash);
        AddShare(share, guardianPublicKey);
    }

    /// <summary>
    /// Create a new instance of a CiphertextDecryptionBallot begining with a collection of shares
    /// </summary>
    public CiphertextDecryptionBallot(
        Dictionary<string, BallotShare> shares,
        Dictionary<string, ElectionPublicKey> guardianPublicKeys)
    {
        BallotId = shares.First().Value.BallotId;
        StyleId = shares.First().Value.StyleId;
        ManifestHash = new(shares.First().Value.ManifestHash);
        Shares = shares.Select(
            x => new KeyValuePair<string, BallotShare>(x.Key, new(x.Value)))
            .ToDictionary(x => x.Key, x => x.Value);
        GuardianPublicKeys = guardianPublicKeys.Select(
            x => new KeyValuePair<string, ElectionPublicKey>(x.Key, new(x.Value)))
            .ToDictionary(x => x.Key, x => x.Value);
    }

    /// <summary>
    /// Add a new share to the collection of shares
    /// </summary>
    public void AddShare(BallotShare share, ElectionPublicKey guardianPublicKey)
    {
        if (!IsValid(share, guardianPublicKey))
        {
            throw new ArgumentException("Invalid share");
        }

        if (!GuardianPublicKeys.ContainsKey(guardianPublicKey.GuardianId))
        {
            GuardianPublicKeys.Add(guardianPublicKey.GuardianId, guardianPublicKey);
        }

        AddShare(share);
    }

    /// <summary>
    /// A convenience accessor to get the collection of shares zipped with the public keys
    /// </summary>

    // TODO: refactor to only return the shares
    public List<Tuple<ElectionPublicKey, BallotShare>> GetShares()
    {
        return Shares.Select(x => new Tuple<ElectionPublicKey, BallotShare>(
            GuardianPublicKeys[x.Key],
            x.Value
        )).ToList();
    }

    /// <summary>
    /// A convenience accessor to get a single share zipped with the public key
    /// </summary>
    public BallotShare? GetShare(string guardianId)
    {
        return Shares[guardianId];
    }

    /// <summary>
    /// check if the share is valid for submission. Used when adding a new share.
    /// </summary>
    public bool IsValid(
        BallotShare share,
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

        return share.GuardianId == guardianPublicKey.GuardianId;
    }

    /// <summary>
    /// check if this class is valid for the ballot. Used when computing decryption.
    /// </summary>
    public bool IsValid(CiphertextBallot ballot, CiphertextElectionContext context)
    {
        var sharesCount = (ulong)Shares.Count;
        if (sharesCount < context.Quorum || sharesCount > context.NumberOfGuardians)
        {
            return false;
        }
        return IsValid(ballot, context.CryptoExtendedBaseHash);
    }

    /// <summary>
    /// check if this class is valid for the ballot.
    /// Compares the ballot id, style id, and manifest hash.
    /// Checks each of the shares in the collection against the guardian public keys.
    /// </summary>
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

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();
        ManifestHash.Dispose();
        GuardianPublicKeys.Dispose();
        foreach (var share in Shares)
        {
            share.Value.Dispose();
        }
    }

    private void AddShare(BallotShare share)
    {
        if (!Shares.ContainsKey(share.GuardianId))
        {
            Shares.Add(share.GuardianId, share);
        }
    }
}
