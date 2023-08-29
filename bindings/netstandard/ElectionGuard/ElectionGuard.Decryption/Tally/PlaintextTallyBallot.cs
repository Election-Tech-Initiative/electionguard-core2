namespace ElectionGuard.Decryption.Tally;

/// <summary>
/// The plaintext representation of all contests in a ballot.
/// </summary>
public record PlaintextTallyBallot : PlaintextTally, IEquatable<PlaintextTallyBallot>
{
    public string BallotId { get; init; } = default!;

    public PlaintextTallyBallot(
        string tallyId, string ballotId, string name, List<ContestDescriptionWithPlaceholders> contests)
        : base(tallyId, name, contests)
    {
        BallotId = ballotId;
    }

    public PlaintextTallyBallot(
        string tallyId, string ballotId, string name, string ballotStyle, InternalManifest manifest)
        : base(tallyId, name, manifest.GetContests(ballotStyle))
    {
        BallotId = ballotId;
    }

    public PlaintextTallyBallot(
        string tallyId,
        CiphertextBallot ballot)
        : base(tallyId, ballot)
    {
        BallotId = ballot.ObjectId;
    }

    public PlaintextTallyBallot(PlaintextTallyBallot ballot)
        : base(ballot)
    {
        BallotId = ballot.BallotId;
    }

    protected PlaintextTallyBallot(PlaintextTally original) : base(original)
    {

    }

    public override string ToString()
    {
        return base.ToString();
    }

    #region IEquatable

    public virtual bool Equals(PlaintextTallyBallot? other)
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
