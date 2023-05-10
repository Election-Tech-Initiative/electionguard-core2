using ElectionGuard.Decryption.Tally;
using ElectionGuard.Guardians;

namespace ElectionGuard.Decryption.Shares;

/// <summary>
/// a share of a guardian's decryption of a collection of contests for a specific ballot (usually a spoiled ballot)
/// </summary>
public record BallotShare : TallyShare, IEquatable<BallotShare>
{
    public string BallotId { get; init; }
    public string StyleId { get; init; }
    public ElementModQ ManifestHash { get; init; }

    public BallotShare(
        string guardianId,
        string tallyId,
        CiphertextBallot ballot,
        Dictionary<string, ContestShare> contests)
        : base(guardianId, tallyId, contests)
    {
        BallotId = ballot.ObjectId;
        StyleId = ballot.StyleId;
        ManifestHash = new(ballot.ManifestHash);
    }

    public BallotShare(BallotShare other) : base(other)
    {
        BallotId = other.BallotId;
        StyleId = other.StyleId;
        ManifestHash = new(other.ManifestHash);
    }

    /// <summary>
    /// Check the validity of the share against the ballot and tally.
    /// </summary>
    public bool IsValid(
        CiphertextBallot ballot,
        ElectionPublicKey guardian,
        CiphertextTally tally)
    {
        return IsValid(ballot, guardian, tally.Context.CryptoExtendedBaseHash);
    }


    /// <summary>
    /// Check the validity of the share against the ballot and election.
    /// </summary>
    public bool IsValid(
        CiphertextBallot ballot,
        ElectionPublicKey guardian,
        ElementModQ extendedBaseHash)
    {
        if (guardian.GuardianId != GuardianId)
        {
            return false;
        }

        if (ballot.ObjectId != BallotId)
        {
            return false;
        }

        if (ballot.StyleId != StyleId)
        {
            return false;
        }

        if (!ballot.ManifestHash.Equals(ManifestHash))
        {
            return false;
        }

        foreach (var contest in ballot.Contests)
        {
            if (!Contests.ContainsKey(contest.ObjectId))
            {
                return false;
            }

            if (!Contests[contest.ObjectId].IsValid(
                contest,
                guardian,
                extendedBaseHash))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// override of the tally base class validity check
    /// that only checks the tally id and guardian id
    /// because we do not have the ciphertext information
    /// </summary>
    public override bool IsValid(
        CiphertextTally tally, ElectionPublicKey guardian)
    {
        if (guardian.GuardianId != GuardianId)
        {
            return false;
        }

        if (tally.TallyId != TallyId)
        {
            return false;
        }

        if (!tally.HasBallot(BallotId))
        {
            return false;
        }

        if (!ManifestHash.Equals(tally.Manifest.ManifestHash))
        {
            return false;
        }

        var contests = tally.Manifest.GetContests(StyleId);

        foreach (var contest in contests)
        {
            if (!Contests.ContainsKey(contest.ObjectId))
            {
                return false;
            }

            // We cannot check if the contest is actually valid in this context
            // because we do not have ciphertext from the tally for the description
        }

        return true;
    }

    /// <summary>
    /// Check the validity of the share without the ciphertext information
    // </summary>
    public bool IsValid(
        string ballotId,
        string styleId,
        ElementModQ manifestHash,
        ElectionPublicKey guardian)
    {
        if (guardian.GuardianId != GuardianId)
        {
            return false;
        }

        if (ballotId != BallotId)
        {
            return false;
        }

        if (styleId != StyleId)
        {
            return false;
        }

        if (!manifestHash.Equals(ManifestHash))
        {
            return false;
        }

        // we cannot check if the contests are valid in this context
        // because we do not have ciphertext information from the parameters

        return true;
    }

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();
        ManifestHash.Dispose();
    }
}
