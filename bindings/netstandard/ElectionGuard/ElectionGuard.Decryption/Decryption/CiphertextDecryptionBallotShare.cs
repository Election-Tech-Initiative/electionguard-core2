namespace ElectionGuard.Decryption.Decryption;

// a share of a guardian's decryption of a collection of contests for a specific ballot (usually a spoiled ballot)
public record CiphertextDecryptionBallotShare : CiphertextDecryptionTallyShare, IEquatable<CiphertextDecryptionTallyShare>
{
    public string BallotId { get; init; }

    public CiphertextDecryptionBallotShare(
        string guardianId,
        string tallyId,
        string ballotId,
        Dictionary<string, CiphertextDecryptionContestShare> contests)
        : base(guardianId, tallyId, contests)
    {
        BallotId = ballotId;
    }
}
