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


public record PlaintextTally : IEquatable<PlaintextTally>
{
    public string TallyId { get; init; } = Guid.NewGuid().ToString();
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
