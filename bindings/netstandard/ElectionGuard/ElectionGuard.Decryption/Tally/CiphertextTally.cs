using ElectionGuard.Encryption.Ballot;
using ElectionGuard.ElectionSetup;
namespace ElectionGuard.Decryption.Tally;

public static partial class InternalManifestExtensions
{
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

public record CiphertextTally : DisposableRecordBase
{
    public string TallyId { get; init; } = Guid.NewGuid().ToString();
    public string Name { get; init; } = default!;

    public CiphertextElectionContext Context { get; init; } = default!;
    public InternalManifest Manifest { get; init; } = default!;

    public HashSet<string> CastBallotIds { get; init; } = default!;
    public HashSet<string> SpoiledBallotIds { get; init; } = default!;

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

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

    public AccumulationResult Accumulate(CiphertextBallot ballot)
    {
        // check for unknown state
        if (ballot.State == BallotBoxState.Unknown)
        {
            throw new ArgumentException("Ballot state is unknown");
        }

        // check for valid ballot
        var isValid = ballot.IsValid(Manifest, Context);
        if (!isValid.IsValid)
        {
            return new AccumulationResult(
                TallyId,
                ballot.ObjectId,
                isValid);
        }

        // add the ballot to the appropriate set
        var added = ballot.IsCast
            ? CastBallotIds.Add(ballot.ObjectId)
            : SpoiledBallotIds.Add(ballot.ObjectId);
        if (!added)
        {
            return new AccumulationResult(
                TallyId,
                ballot.ObjectId,
                new BallotValidationResult(
                    $"Ballot {ballot.ObjectId} already added to tally {TallyId}"));
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

    public AccumulationResult Accumulate(List<CiphertextBallot> ballots)
    {
        var result = new AccumulationResult(TallyId);
        foreach (var ballot in ballots)
        {
            var ballotResult = Accumulate(ballot);
            result.Add(ballotResult);
        }
        return result;
    }

    public async Task<AccumulationResult> AccumulateAsync(CiphertextBallot ballot)
    {
        // check for unknown state
        if (ballot.State == BallotBoxState.Unknown)
        {
            throw new ArgumentException("Ballot state is unknown");
        }

        // check for valid ballot
        var isValid = ballot.IsValid(Manifest, Context);
        if (!isValid.IsValid)
        {
            return new AccumulationResult(
                TallyId,
                ballot.ObjectId,
                isValid);
        }

        // add the ballot to the appropriate set
        var added = ballot.IsCast
            ? CastBallotIds.Add(ballot.ObjectId)
            : SpoiledBallotIds.Add(ballot.ObjectId);
        if (!added)
        {
            return new AccumulationResult(
                TallyId,
                ballot.ObjectId,
                new BallotValidationResult(
                    $"Ballot {ballot.ObjectId} already added to tally {TallyId}"));
        }

        // accumulate the contests
        var tasks = new List<Task>();
        if (ballot.IsCast)
        {
            foreach (var contest in ballot.Contests)
            {
                tasks.Add(Contests[contest.ObjectId].AccumulateAsync(contest));
            }
        }
        await Task.WhenAll(tasks);

        return new AccumulationResult(TallyId, ballot.ObjectId);
    }

    public async Task<AccumulationResult> AccumulateAsync(List<CiphertextBallot> ballots)
    {
        var result = new AccumulationResult(TallyId);
        var tasks = new List<Task<AccumulationResult>>();
        foreach (var ballot in ballots)
        {
            tasks.Add(AccumulateAsync(ballot));
        }

        var taskResults = await Task.WhenAll(tasks);

        foreach (var item in taskResults)
        {
            result.Add(item);
        }

        return result;
    }
}
