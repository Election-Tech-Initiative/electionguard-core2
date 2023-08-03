using System.Text;
using ElectionGuard.Decryption.Decryption;
using ElectionGuard.ElectionSetup;
using ElectionGuard.ElectionSetup.Extensions;

namespace ElectionGuard.Decryption.Tally;

public static partial class InternalManifestExtensions
{
    public static Dictionary<string, PlaintextTallyContest> ToPlaintextTallyContestDictionary(
        this InternalManifest manifest)
    {
        var contests = new Dictionary<string, PlaintextTallyContest>();
        foreach (var contestDescription in manifest.Contests)
        {
            contests.Add(
                contestDescription.ObjectId,
                new PlaintextTallyContest(contestDescription));
        }
        return contests;
    }

    public static Dictionary<string, PlaintextTallyContest> ToPlaintextTallyContestDictionary(
        this InternalManifest manifest, string ballotStyle)
    {
        var contests = new Dictionary<string, PlaintextTallyContest>();
        foreach (var contestDescription in manifest.GetContests(ballotStyle))
        {
            contests.Add(
                contestDescription.ObjectId,
                new PlaintextTallyContest(contestDescription));
        }
        return contests;
    }

    public static Dictionary<string, PlaintextTallyContest> ToPlaintextTallyContestDictionary(
       this CiphertextBallot ballot)
    {
        var contests = new Dictionary<string, PlaintextTallyContest>();
        foreach (var contest in ballot.Contests)
        {
            contests.Add(
                contest.ObjectId,
                new PlaintextTallyContest(contest));
        }
        return contests;
    }
}

/// <summary>
/// The plaintext representation of all contests in the election.
/// </summary>
public record PlaintextTally : DisposableRecordBase, IEquatable<PlaintextTally>
{
    /// <summary>
    /// The unique identifier for the tally. May be the same as the ciphertext tally id.
    /// </summary>
    public string TallyId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The name of the tally
    /// </summary>
    public string Name { get; init; } = default!;

    /// <summary>
    /// A collection of each contest and selection in an election.
    /// Retains an encrypted representation of a tally for each selection
    /// </summary>
    public Dictionary<string, PlaintextTallyContest> Contests { get; init; } = new();

    public PlaintextTally(
        string name,
        InternalManifest manifest)
    {
        Name = name;
        Contests = manifest.ToPlaintextTallyContestDictionary();
    }

    public PlaintextTally(
        string tallyId,
        string name,
        InternalManifest manifest)
    {
        TallyId = tallyId;
        Name = name;
        Contests = manifest.ToPlaintextTallyContestDictionary();
    }

    public PlaintextTally(
        string tallyId,
        CiphertextBallot ballot)
    {
        TallyId = tallyId;
        Name = ballot.BallotCode.ToHex();
        Contests = ballot.ToPlaintextTallyContestDictionary();
    }

    [JsonConstructor]
    public PlaintextTally(
        string tallyId,
        string name,
        Dictionary<string, PlaintextTallyContest> contests)
    {
        TallyId = tallyId;
        Name = name;

        foreach (var contestId in contests.Keys)
        {
            Contests.Add(
                contestId,
                new PlaintextTallyContest(contests[contestId]));
        }
    }

    public PlaintextTally(
        string tallyId,
        string name,
        List<ContestDescriptionWithPlaceholders> contests)
    {
        TallyId = tallyId;
        Name = name;

        foreach (var contestDescription in contests)
        {
            Contests.Add(
                contestDescription.ObjectId,
                new PlaintextTallyContest(contestDescription));
        }
    }

    public PlaintextTally(PlaintextTally other) : base(other)
    {
        TallyId = other.TallyId;
        Name = other.Name;
        Contests = other.Contests
            .ToDictionary(
                kvp => kvp.Key,
                kvp => new PlaintextTallyContest(kvp.Value));
    }

    protected override void DisposeManaged()
    {
        base.DisposeManaged();
        Contests?.Dispose();
    }

    public static implicit operator string(PlaintextTally self)
    {
        return self.ToString();
    }

    public static implicit operator DecryptionResult(PlaintextTally self)
    {
        return new DecryptionResult(self.TallyId, self);
    }

    public override string ToString()
    {
        var builder = new StringBuilder();
        _ = builder.AppendLine($"TallyId: {TallyId}");
        _ = builder.AppendLine($"Name: {Name}");
        _ = builder.AppendLine($"Contests: {Contests.Count}");
        foreach (var contest in Contests)
        {
            _ = builder.AppendLine($"  Contest: {contest.Key}");
            _ = builder.AppendLine($"    Selections: {contest.Value.Selections.Count}");
            foreach (var selection in contest.Value.Selections)
            {
                _ = builder.AppendLine($"      Selection: {selection.Key}");
                _ = builder.AppendLine($"        Votes: {selection.Value.Tally}");
            }
        }
        return builder.ToString();
    }

    #region IEquatable

    public virtual bool Equals(PlaintextTally? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return TallyId == other.TallyId &&
               Name == other.Name &&
               Contests.SequenceEqual(other.Contests);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(TallyId, Name, Contests);
    }

    #endregion
}
