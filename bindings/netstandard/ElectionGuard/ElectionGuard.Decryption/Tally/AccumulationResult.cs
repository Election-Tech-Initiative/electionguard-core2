using System.Text;
using ElectionGuard.Encryption.Ballot;

namespace ElectionGuard.Decryption.Tally;

/// <summary>
/// The result of a ciphertext tally accumulation operation.
/// </summary>
public class AccumulationResult
{
    /// <summary>
    /// The id of the tally
    /// </summary>
    public string TallyId { get; init; }

    /// <summary>
    /// The set of ballot ids that were successfully accumulated
    /// </summary>
    public HashSet<string> Accumulated { get; init; }

    /// <summary>
    /// The set of ballot ids that failed to accumulate along with the reason
    /// </summary>
    public Dictionary<string, BallotValidationResult> Failed { get; init; }

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

    /// <summary>
    /// Add the results of another accumulation operation to this one
    /// </summary>
    public AccumulationResult Add(AccumulationResult other)
    {
        // check the other accumulated ballots and verify there are no duplicates
        foreach (var ballotId in other.Accumulated)
        {
            if (Accumulated.Contains(ballotId))
            {
                throw new ArgumentException($"Ballot {ballotId} already added to tally {TallyId}");
            }
        }

        Accumulated.UnionWith(other.Accumulated);
        foreach (var (ballotId, validationResult) in other.Failed)
        {
            Failed.Add(ballotId, validationResult);
        }

        return this;
    }

    public override string ToString()
    {
        var builder = new StringBuilder();
        _ = builder.AppendLine($"Tally {TallyId}");
        _ = builder.AppendLine($"Accumulated {Accumulated.Count} ballots");
        _ = builder.AppendLine($"Failed {Failed.Count} ballots");
        foreach (var (ballotId, validationResult) in Failed)
        {
            _ = builder.AppendLine($"- Ballot {ballotId}: {validationResult}");
        }
        return builder.ToString();
    }
}
