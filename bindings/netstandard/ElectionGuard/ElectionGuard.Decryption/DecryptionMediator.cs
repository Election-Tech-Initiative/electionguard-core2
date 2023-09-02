using ElectionGuard.Decryption.Challenge;
using ElectionGuard.Decryption.ChallengeResponse;
using ElectionGuard.Decryption.Decryption;
using ElectionGuard.Decryption.Shares;
using ElectionGuard.Decryption.Tally;
using ElectionGuard.ElectionSetup.Extensions;
using ElectionGuard.Guardians;

namespace ElectionGuard.Decryption;

/// <summary>
/// A mediator for coordinating decryption
/// </summary>
public class DecryptionMediator : DisposableBase
{
    public string Id { get; }

    // guardians that are present for this decryption session
    public Dictionary<string, ElectionPublicKey> Guardians { get; init; } = new Dictionary<string, ElectionPublicKey>();

    // key is tally id
    public Dictionary<string, CiphertextTally> Tallies { get; init; } = new Dictionary<string, CiphertextTally>();

    // key is tally id
    public Dictionary<string, CiphertextDecryptionTally> TallyDecryptions { get; init; } = new Dictionary<string, CiphertextDecryptionTally>();

    public DecryptionMediator(string mediatorId)
    {
        Id = mediatorId;
    }

    public DecryptionMediator(
        string mediatorId,
        CiphertextTally tally,
        List<ElectionPublicKey> guardians)
    {
        Id = mediatorId;

        Tallies.Add(tally.TallyId, new(tally));
        TallyDecryptions.Add(
                tally.TallyId,
                new CiphertextDecryptionTally(Tallies[tally.TallyId]));
        foreach (var guardian in guardians)
        {
            AddGuardian(guardian);
        }
    }

    public DecryptionMediator(
        string mediatorId,
        CiphertextTally tally,
        List<ElectionPublicKey> guardians,
        List<CiphertextBallot> ballots) : this(mediatorId, tally, guardians)
    {
        foreach (var ballot in ballots)
        {
            AddBallot(tally.TallyId, ballot);
        }
    }


    #region Misc Admin

    public void AddBallots(
        string tallyId, List<CiphertextBallot> ballots)
    {
        TallyDecryptions[tallyId].AddBallots(ballots);
    }

    public void AddBallot(string tallyId, CiphertextBallot ballot)
    {
        TallyDecryptions[tallyId].AddBallot(ballot);
    }

    public void AddTally(CiphertextTally tally)
    {
        Tallies.Add(tally.TallyId, new(tally));
    }

    public void AddGuardian(ElectionPublicKey guardian)
    {
        Guardians.Add(guardian.GuardianId, new(guardian));
    }

    #endregion

    #region Submit Shares

    public GuardianShare? GetShare(string tallyId, string guardianId)
    {
        if (!TallyDecryptions.ContainsKey(tallyId))
        {
            return null;
        }

        var tallyDecryption = TallyDecryptions[tallyId];
        return tallyDecryption.GetShare(guardianId);
    }

    /// <summary>
    /// Submit a single tally share
    /// </summary>
    public void SubmitShare(
        TallyShare tallyShare)
    {
        EnsureCiphertextDecryptionTally(tallyShare);

        if (!tallyShare.IsValid(
            Tallies[tallyShare.TallyId], Guardians[tallyShare.GuardianId]))
        {
            throw new Exception("Tally share is not valid");
        }

        var tallyDecryption = TallyDecryptions[tallyShare.TallyId];
        tallyDecryption.AddTallyShare(
            Guardians[tallyShare.GuardianId], new(tallyShare));
    }

    /// <summary>
    /// Submit a single ballot share
    /// </summary>
    protected void SubmitShare(
        BallotShare ballotShare,
        CiphertextBallot ballot)
    {
        EnsureCiphertextDecryptionTally(ballotShare);

        if (!ballotShare.IsValid(
            ballot,
            Guardians[ballotShare.GuardianId],
            Tallies[ballotShare.TallyId]))
        {
            throw new Exception("Tally share is not valid");
        }

        var tallyDecryption = TallyDecryptions[ballotShare.TallyId];
        tallyDecryption.AddBallotShare(
            Guardians[ballotShare.GuardianId], new(ballotShare), new(ballot));
    }

    /// <summary>
    /// Submit a tally share and a list of ballot shares
    /// </summary>
    public void SubmitShares(
        DecryptionShare shares,
        List<CiphertextBallot> ballots
         )
    {
        SubmitShare(shares.TallyShare);
        if (ballots.Count > 0)
        {
            SubmitShares(
                shares.BallotShares.Values.Select(i => new BallotShare(i)).ToList(),
                ballots.Select(i => new CiphertextBallot(i)).ToList());
        }
    }

    /// <summary>
    /// Submit a list of tally shares and a list of ballot shares
    /// </summary>
    public void SubmitShares(
        TallyShare tallyShare,
        List<BallotShare> ballotShares,
        List<CiphertextBallot> ballots)
    {
        SubmitShare(tallyShare);
        SubmitShares(ballotShares, ballots);
    }

    /// <summary>
    /// Submit a list of ballot shares
    /// </summary>
    public void SubmitShares(
        List<BallotShare> ballotShares,
        List<CiphertextBallot> ballots)
    {
        if (ballotShares.Count == 0)
        {
            throw new Exception("No ballot shares provided");
        }

        if (ballotShares.Count != ballots.Count)
        {
            throw new Exception("Number of ballot shares does not match number of ballots");
        }

        EnsureCiphertextDecryptionTally(ballotShares.First());

        foreach (var ballotShare in ballotShares)
        {
            if (!ballotShare.IsValid(
                ballots.First(i => i.ObjectId == ballotShare.BallotId),
                 Guardians[ballotShare.GuardianId],
                 Tallies[ballotShare.TallyId]))
            {
                throw new Exception("Tally share is not valid");
            }
        }

        var tallyDecryption = TallyDecryptions[ballotShares.First().TallyId];
        var guardian = Guardians[ballotShares.First().GuardianId];
        foreach (var ballotShare in ballotShares)
        {
            tallyDecryption.AddBallotShare(
                guardian, ballotShare,
                ballots.First(i => i.ObjectId == ballotShare.BallotId));
        }
    }

    #endregion

    #region Accumulate Tally

    // accumulate can be called before CreateChallenge
    // but it doesnt have to be because CreateChallenge will also handle it
    public void AccumulateShares(string tallyId, bool skipValidation = false)
    {
        _ = TallyDecryptions[tallyId].AccumulateShares(skipValidation);
    }

    #endregion

    #region Challenge

    public Dictionary<string, GuardianChallenge> CreateChallenge(string tallyId, bool skipValidation = false)
    {
        return TallyDecryptions[tallyId].CreateChallenge(skipValidation);
    }
    public void LoadChallenge(string tallyId, string guardianId, GuardianChallenge challenge)
    {
        TallyDecryptions[tallyId].LoadChallenge(guardianId, challenge);
    }

    #endregion

    #region Challenge Response

    public void SubmitResponse(
        string tallyId, GuardianChallengeResponse response)
    {
        TallyDecryptions[tallyId].SubmitChallengeResponse(response);
    }

    public bool ValidateResponses(string tallyId)
    {
        return TallyDecryptions[tallyId].ValidateChallengeResponses();
    }

    // compute proofs can be called before decrypt
    // but it doesnt have to be because decrypt will also handle it
    public void ComputeProofs(string tallyId, bool skipValidation = false)
    {
        TallyDecryptions[tallyId].ComputeDecryptionProofs(skipValidation);
    }

    #endregion

    #region Decrypt

    /// <summary>
    /// Determine if the tally can be decrypted.
    /// </summary>
    public DecryptionResult CanDecrypt(string tallyId)
    {
        if (!Tallies.ContainsKey(tallyId))
        {
            return new DecryptionResult(tallyId, "Tally does not exist");
        }

        if (!TallyDecryptions.ContainsKey(tallyId))
        {
            return new DecryptionResult(tallyId, "Tally decryption does not exist");
        }

        return new DecryptionResult(tallyId);
    }

    /// <summary>
    /// Decrypt the tally
    /// </summary>
    public DecryptionResult Decrypt(string tallyId, bool skipValidation = false)
    {
        var canDecrypt = CanDecrypt(tallyId);
        if (!canDecrypt)
        {
            return canDecrypt;
        }

        var tallyDecryption = TallyDecryptions[tallyId];
        return tallyDecryption.Decrypt(false, skipValidation);
    }

    #endregion

    protected override void DisposeManaged()
    {
        base.DisposeManaged();
        Guardians.Dispose();
        Tallies.Dispose();
        TallyDecryptions.Dispose();
    }

    private void EnsureCiphertextDecryptionTally(TallyShare share)
    {
        if (!Tallies.ContainsKey(share.TallyId))
        {
            throw new ArgumentException($"Tally does not exist {share.TallyId}");
        }

        if (!Guardians.ContainsKey(share.GuardianId))
        {
            throw new ArgumentException($"Guardian does not exist {share.GuardianId}");
        }

        if (!TallyDecryptions.ContainsKey(share.TallyId))
        {
            TallyDecryptions.Add(
                share.TallyId,
                new CiphertextDecryptionTally(Tallies[share.TallyId]));
        }
    }
}
