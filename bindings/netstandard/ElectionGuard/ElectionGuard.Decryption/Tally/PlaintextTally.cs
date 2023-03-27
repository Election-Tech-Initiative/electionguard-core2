using System.Text;

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
}

/// <summary>
/// The plaintext representation of all contests in the election.
/// </summary>
public record PlaintextTally : IEquatable<PlaintextTally>
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
    public Dictionary<string, PlaintextTallyContest> Contests { get; init; } = default!;

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
