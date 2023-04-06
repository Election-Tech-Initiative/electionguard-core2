using ElectionGuard.Decryption.Tally;
using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.Decryption.Decryption;

// a share of a guardian's decryption of a collection of contests for a specific ballot (usually a spoiled ballot)
public record CiphertextDecryptionBallotShare : CiphertextDecryptionTallyShare, IEquatable<CiphertextDecryptionTallyShare>
{
    public string BallotId { get; init; }
    public string StyleId { get; init; }
    public ElementModQ ManifestHash { get; init; }

    public CiphertextDecryptionBallotShare(
        string guardianId,
        string tallyId,
        CiphertextBallot ballot,
        Dictionary<string, CiphertextDecryptionContestShare> contests)
        : base(guardianId, tallyId, contests)
    {
        BallotId = ballot.ObjectId;
        StyleId = ballot.StyleId;
        ManifestHash = ballot.ManifestHash;
    }

    public bool IsValid(
        CiphertextBallot ballot,
        ElectionPublicKey guardian,
        CiphertextTally tally)
    {
        return IsValid(ballot, guardian, tally.Context.CryptoExtendedBaseHash);
    }

    // check the validity of the share with the ciphertext information
    public bool IsValid(
        CiphertextBallot ballot,
        ElectionPublicKey guardian,
        ElementModQ extendedBaseHash)
    {
        if (guardian.OwnerId != GuardianId)
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

    // override of the tally base class validity check
    // that only checks the tally id and guardian id
    // because we do not have the ciphertext information
    public override bool IsValid(
        CiphertextTally tally, ElectionPublicKey guardian)
    {
        if (guardian.OwnerId != GuardianId)
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

    // check the validity of the share without the ciphertext information
    public bool IsValid(
        string ballotId,
        string styleId,
        ElementModQ manifestHash,
        ElectionPublicKey guardian)
    {
        if (guardian.OwnerId != GuardianId)
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
}
