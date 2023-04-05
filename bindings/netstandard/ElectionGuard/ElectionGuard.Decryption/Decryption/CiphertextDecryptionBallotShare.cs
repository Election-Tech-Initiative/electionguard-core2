namespace ElectionGuard.Decryption.Decryption;

// a share of a guardian's decryption of a collection of contests for a specific ballot (usually a spoiled ballot)
public record CiphertextDecryptionBallotShare : CiphertextDecryptionTallyShare, IEquatable<CiphertextDecryptionTallyShare>
{
    public string BallotId { get; init; }
    public string StyleId { get; init; }
    public ElementModQ ManifestHash { get; init; }

    public CiphertextDecryptionBallotShare(
        string guardianId,
        string tallyId,
        CiphertextBallot ballot,
        Dictionary<string, CiphertextDecryptionContestShare> contests)
        : base(guardianId, tallyId, contests)
    {
        BallotId = ballot.ObjectId;
        StyleId = ballot.StyleId;
        ManifestHash = ballot.ManifestHash;
    }

    public CiphertextDecryptionBallotShare(
        string guardianId,
        string tallyId,
        string ballotId,
        string styleId,
        ElementModQ manifestHash,
        Dictionary<string, CiphertextDecryptionContestShare> contests)
        : base(guardianId, tallyId, contests)
    {
        BallotId = ballotId;
        StyleId = styleId;
        ManifestHash = manifestHash;
    }
}
