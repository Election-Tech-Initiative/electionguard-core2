using ElectionGuard.Encryption.Ballot;
using ElectionGuard.ElectionSetup;
using System.Text;

namespace ElectionGuard.Decryption.Tally;

/// <summary>
/// The encrypted representation of all contests in the election.
/// A `CiphertextTally` accepts cast and spoiled ballots and accumulates a tally on the cast ballots.
/// </summary>
public record CiphertextTally : DisposableRecordBase, IEquatable<CiphertextTally>
{
    /// <summary>
    /// The unique identifier for the tally.
    /// </summary>
    public string TallyId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The name of the tally.
    /// </summary>
    public string Name { get; init; } = default!;

    /// <summary>
    /// The election context.
    /// </summary>
    public CiphertextElectionContext Context { get; init; } = default!;

    /// <summary>
    /// The election manifest.
    /// </summary>
    public InternalManifest Manifest { get; init; } = default!;

    /// <summary>
    /// a set of cast ballot ids cast in the election.
    /// </summary>
    public HashSet<string> CastBallotIds { get; init; } = default!;

    /// <summary>
    /// a set of spoiled ballot ids cast in the election.
    /// </summary>
    public HashSet<string> SpoiledBallotIds { get; init; } = default!;

    /// <summary>
    /// A collection of each contest and selection in an election.
    /// Retains an encrypted representation of a tally for each selection
    /// </summary>
    public Dictionary<string, CiphertextTallyContest> Contests { get; init; } = default!;

    public CiphertextTally(
        string name,
        CiphertextElectionContext context,
        InternalManifest manifest)
    {
        Name = name;
        Context = context;
        Manifest = manifest;
        CastBallotIds = new HashSet<string>();
        SpoiledBallotIds = new HashSet<string>();
        Contests = manifest.ToCiphertextTallyContestDictionary();
    }

    public CiphertextTally(
        string tallyId,
        string name,
        CiphertextElectionContext context,
        InternalManifest manifest)
    {
        TallyId = tallyId;
        Name = name;
        Context = context;
        Manifest = manifest;
        CastBallotIds = new HashSet<string>();
        SpoiledBallotIds = new HashSet<string>();
        Contests = manifest.ToCiphertextTallyContestDictionary();
    }

    public CiphertextTally(
        string tallyId,
        string name,
        CiphertextElectionContext context,
        InternalManifest manifest,
        Dictionary<string, CiphertextTallyContest> contests)
    {
        TallyId = tallyId;
        Name = name;
        Context = context;
        Manifest = manifest;
        CastBallotIds = new HashSet<string>();
        SpoiledBallotIds = new HashSet<string>();
        Contests = contests;
    }

    /// <summary>
    /// Add a ballot to the tally and recalculate the tally.
    /// </summary>
    public AccumulationResult Accumulate(
        CiphertextBallot ballot, bool skipValidation = false)
    {
        // add the ballot to the cast or spoil collection
        var addResult = TryAddBallot(ballot, skipValidation);
        if (!addResult.IsValid)
        {
            return new AccumulationResult(TallyId, ballot.ObjectId, addResult);
        }

        // accumulate the contests
        if (ballot.IsCast)
        {
            Console.WriteLine($"Accumulating ballot {ballot.ObjectId} style: {ballot.StyleId} contests: {ballot.Contests.Count}");
            foreach (var contest in ballot.Contests)
            {
                Contests[contest.ObjectId].Accumulate(contest);
            }
        }

        return new AccumulationResult(TallyId, ballot.ObjectId);
    }

    /// <summary>
    /// Add a list of ballots to the tally and recalculate the tally.
    /// </summary>
    public AccumulationResult Accumulate(
        List<CiphertextBallot> ballots, bool skipValidation = false)
    {
        var result = new AccumulationResult(TallyId);
        foreach (var ballot in ballots)
        {
            var ballotResult = Accumulate(ballot, skipValidation);
            _ = result.Add(ballotResult);
        }
        return result;
    }

    /// <summary>
    /// Add a ballot to the tally and recalculate the tally.
    /// </summary>
    public async Task<AccumulationResult> AccumulateAsync(
        CiphertextBallot ballot, bool skipValidation = false,
        CancellationToken cancellationToken = default)
    {
        // add the ballot to the cast or spoil collection
        var addResult = TryAddBallot(ballot, skipValidation);
        if (!addResult.IsValid)
        {
            return new AccumulationResult(TallyId, ballot.ObjectId, addResult);
        }

        // accumulate the contests
        var tasks = new List<Task>();
        if (ballot.IsCast)
        {
            foreach (var contest in ballot.Contests)
            {
                tasks.Add(Contests[contest.ObjectId].AccumulateAsync(
                    contest, cancellationToken));
            }
        }
        await Task.WhenAll(tasks);

        return new AccumulationResult(TallyId, ballot.ObjectId);
    }

    /// <summary>
    /// Add a list of ballots to the tally and recalculate the tally.
    /// </summary>
    public async Task<AccumulationResult> AccumulateAsync(
        List<CiphertextBallot> ballots,
        bool skipValidation = false,
        CancellationToken cancellationToken = default)
    {
        var result = new AccumulationResult(TallyId);
        foreach (var ballot in ballots)
        {
            _ = result.Add(
                await AccumulateAsync(
                    ballot, skipValidation, cancellationToken));
        }

        return result;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        _ = sb.AppendLine($"    TallyId: {TallyId}");
        _ = sb.AppendLine($"    Name: {Name}");

        _ = sb.AppendLine($"    CryptoExtendedBaseHash: {Context.CryptoExtendedBaseHash}");
        _ = sb.AppendLine($"    ManifestHash: {Manifest.ManifestHash}");

        _ = sb.AppendLine($"    - CastBallotIds: {CastBallotIds.Count}");
        _ = sb.AppendLine($"    - SpoiledBallotIds: {SpoiledBallotIds.Count}");
        return sb.ToString();
    }

    #region Equality Overrides

    public virtual bool Equals(CiphertextTally? other)
    {
        return other != null &&
               TallyId == other.TallyId &&
               Name == other.Name &&
               Context.CryptoExtendedBaseHash == other.Context.CryptoExtendedBaseHash &&
               Manifest.ManifestHash == other.Manifest.ManifestHash &&
               CastBallotIds.SetEquals(other.CastBallotIds) &&
               SpoiledBallotIds.SetEquals(other.SpoiledBallotIds) &&
               Contests.SequenceEqual(other.Contests);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            TallyId, Name, Context, Manifest, CastBallotIds, SpoiledBallotIds, Contests);
    }

    #endregion Equality Overrides

    /// <summary>
    /// try to add the ballot to the cast and spoiled collection 
    /// to indicate that it is a new ballot being added to the tally
    /// </summary>
    private BallotValidationResult TryAddBallot(
        CiphertextBallot ballot, bool skipValidation = false)
    {
        // check for unknown state
        if (ballot.State == BallotBoxState.Unknown)
        {
            throw new ArgumentException("Ballot state is unknown");
        }

        // check for valid ballot
        var isValid = skipValidation
            ? new BallotValidationResult(true)
            : ballot.IsValid(Manifest, Context);
        if (!isValid.IsValid)
        {
            return isValid;
        }

        // add the ballot to the appropriate set
        var added = ballot.IsCast
            ? CastBallotIds.Add(ballot.ObjectId)
            : SpoiledBallotIds.Add(ballot.ObjectId);
        if (!added)
        {
            return new BallotValidationResult(
                    $"Ballot {ballot.ObjectId} already added to tally {TallyId}");
        }

        return new BallotValidationResult(true);
    }
}

public static partial class InternalManifestExtensions
{
    /// <summary>
    /// Converts an internal manifest to a dictionary of CiphertextTallyContest
    /// </summary>
    public static Dictionary<string, CiphertextTallyContest> ToCiphertextTallyContestDictionary(
        this InternalManifest manifest)
    {
        var contests = new Dictionary<string, CiphertextTallyContest>();
        foreach (var contestDescription in manifest.Contests)
        {
            contests.Add(
                contestDescription.ObjectId,
                new CiphertextTallyContest(contestDescription));
        }
        return contests;
    }
}
