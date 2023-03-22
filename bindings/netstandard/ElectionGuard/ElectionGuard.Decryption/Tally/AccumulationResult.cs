using ElectionGuard.Encryption.Ballot;

namespace ElectionGuard.Decryption.Tally;

public struct AccumulationResult
{
    public string TallyId { get; init; } = default!;
    public HashSet<string> Accumulated { get; init; } = default!;
    public Dictionary<string, BallotValidationResult> Failed { get; init; } = default!;

    public AccumulationResult(string tallyId)
    {
        TallyId = tallyId;
        Accumulated = new HashSet<string>();
        Failed = new Dictionary<string, BallotValidationResult>();
    }

    public AccumulationResult(
        string tallyId,
        string ballotId)
    {
        TallyId = tallyId;
        Accumulated = new HashSet<string>() { ballotId };
        Failed = new Dictionary<string, BallotValidationResult>();
    }

    public AccumulationResult(
        string tallyId,
        HashSet<string> accumulatedBallotIds)
    {
        TallyId = tallyId;
        Accumulated = accumulatedBallotIds;
        Failed = new Dictionary<string, BallotValidationResult>();
    }

    public AccumulationResult(
        string tallyId,
        Dictionary<string, BallotValidationResult> failedBallotIds)
    {
        TallyId = tallyId;
        Accumulated = new HashSet<string>();
        Failed = failedBallotIds;
    }

    public AccumulationResult(
        string tallyId,
        HashSet<string> accumulatedBallotIds,
        Dictionary<string, BallotValidationResult> failedBallotIds)
    {
        TallyId = tallyId;
        Accumulated = accumulatedBallotIds;
        Failed = failedBallotIds;
    }

    public AccumulationResult(
        string tallyId,
        string ballotId,
        BallotValidationResult validationResult)
    {
        TallyId = tallyId;
        Accumulated = new HashSet<string>();
        Failed = new Dictionary<string, BallotValidationResult>
        {
            { ballotId, validationResult }
        };
    }

    public AccumulationResult(
        string tallyId,
        HashSet<string> accumulatedBallotIds,
        string ballotId,
        BallotValidationResult validationResult)
    {
        TallyId = tallyId;
        Accumulated = accumulatedBallotIds;
        Failed = new Dictionary<string, BallotValidationResult>
        {
            { ballotId, validationResult }
        };
    }

    public void Add(AccumulationResult other)
    {
        Accumulated.UnionWith(other.Accumulated);
        foreach (var (ballotId, validationResult) in other.Failed)
        {
            Failed.Add(ballotId, validationResult);
        }
    }
}
