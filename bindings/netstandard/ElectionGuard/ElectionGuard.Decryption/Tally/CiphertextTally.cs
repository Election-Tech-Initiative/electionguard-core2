using ElectionGuard.ElectionSetup;
using System.Text;
using ElectionGuard.ElectionSetup.Extensions;
using System.Diagnostics;
using Newtonsoft.Json;
using ElectionGuard.Ballot;

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
    public HashSet<string> CastBallotIds { get; init; } = new HashSet<string>();

    /// <summary>
    /// a set of spoiled ballot ids cast in the election.
    /// </summary>
    public HashSet<string> ChallengedBallotIds { get; init; } = new HashSet<string>();

    public HashSet<string> SpoiledBallotIds { get; init; } = new HashSet<string>();

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
        Context = new(context);
        Manifest = new(manifest);
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
        Context = new(context);
        Manifest = new(manifest);
        Contests = manifest.ToCiphertextTallyContestDictionary();
    }

    [JsonConstructor]
    public CiphertextTally(
        string tallyId,
        string name,
        CiphertextElectionContext context,
        InternalManifest manifest,
        HashSet<string> castBallotIds,
        HashSet<string> challengedBallotIds,
        HashSet<string> spoiledBallotIds,
        Dictionary<string, CiphertextTallyContest> contests)
    {
        TallyId = tallyId;
        Name = name;
        Context = new(context);
        Manifest = new(manifest);
        CastBallotIds = new HashSet<string>(castBallotIds);
        ChallengedBallotIds = new HashSet<string>(challengedBallotIds);
        SpoiledBallotIds = new HashSet<string>(spoiledBallotIds);
        Contests = contests.Select(
            entry => new KeyValuePair<string, CiphertextTallyContest>(
                entry.Key,
                new CiphertextTallyContest(entry.Value))).ToDictionary(
                    entry => entry.Key,
                    entry => entry.Value);
    }

    public CiphertextTally(CiphertextTally other) : base(other)
    {
        TallyId = other.TallyId;
        Name = other.Name;
        Context = new(other.Context);
        Manifest = new(other.Manifest);
        CastBallotIds = new HashSet<string>(other.CastBallotIds);
        ChallengedBallotIds = new HashSet<string>(other.ChallengedBallotIds);
        SpoiledBallotIds = new HashSet<string>(other.SpoiledBallotIds);
        Contests = other.Contests.Select(
            x => new KeyValuePair<string, CiphertextTallyContest>(
                x.Key, new CiphertextTallyContest(x.Value)))
                .ToDictionary(
            entry => entry.Key,
            entry => new CiphertextTallyContest(entry.Value));
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

    public bool HasBallot(string ballotId)
    {
        return CastBallotIds.Contains(ballotId) ||
               ChallengedBallotIds.Contains(ballotId) ||
               SpoiledBallotIds.Contains(ballotId);
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        _ = sb.AppendLine($"    TallyId: {TallyId}");
        _ = sb.AppendLine($"    Name: {Name}");

        _ = sb.AppendLine($"    CryptoExtendedBaseHash: {Context.CryptoExtendedBaseHash}");
        _ = sb.AppendLine($"    ManifestHash: {Manifest.ManifestHash}");

        _ = sb.AppendLine($"    - CastBallotIds: {CastBallotIds.Count}");
        _ = sb.AppendLine($"    - ChallengedBallotIds: {ChallengedBallotIds.Count}");
        _ = sb.AppendLine($"    - SpoiledBallotIds: {SpoiledBallotIds.Count}");
        return sb.ToString();
    }

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();
        Manifest?.Dispose();
        Context?.Dispose();
        Contests?.Dispose();
        Contests?.Clear();
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
               ChallengedBallotIds.SetEquals(other.ChallengedBallotIds) &&
               SpoiledBallotIds.SetEquals(other.SpoiledBallotIds) &&
               Contests.SequenceEqual(other.Contests);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            TallyId, Name, Context, Manifest,
            CastBallotIds, ChallengedBallotIds, SpoiledBallotIds,
            Contests);
    }

    #endregion Equality Overrides

    /// <summary>
    /// try to add the ballot to the cast and spoiled collection 
    /// to indicate that it is a new ballot being added to the tally
    /// </summary>
    private BallotValidationResult TryAddBallot(
        CiphertextBallot ballot, bool skipValidation = false)
    {
        // check for valid ballot
        var isValid = skipValidation
            ? new BallotValidationResult(true)
            : ballot.IsValid(Manifest, Context);
        if (!isValid.IsValid)
        {
            Debug.WriteLine($"Ballot {ballot.ObjectId} is not valid");
            return isValid;
        }
        var added = ballot.State switch
        {
            BallotBoxState.Spoiled => SpoiledBallotIds.Add(ballot.ObjectId),
            BallotBoxState.Challenged => ChallengedBallotIds.Add(ballot.ObjectId),
            BallotBoxState.Cast => CastBallotIds.Add(ballot.ObjectId),
            _ => throw new ArgumentException($"Incorrect ballot state {ballot.State}"),
        };

        return !added
            ? new BallotValidationResult(
                    $"Ballot {ballot.ObjectId} already added to tally {TallyId}")
            : new BallotValidationResult(true);
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
