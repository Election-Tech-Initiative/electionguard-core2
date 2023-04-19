using System;
using System.Collections.Generic;
using System.Linq;
using ElectionGuard;
using ElectionGuard.UI.Lib.Models;
using ElectionGuard.Decryption.Tally;

namespace ElectionGuard.Decryption.Decryption;

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

    public void AddBallot(string tallyId, CiphertextBallot ballot)
    {
        TallyDecryptions[tallyId].AddBallot(new(ballot));
    }

    public void AddTally(CiphertextTally tally)
    {
        Tallies.Add(tally.TallyId, new(tally));
    }

    public void AddGuardian(ElectionPublicKey guardian)
    {
        Guardians.Add(guardian.OwnerId, new(guardian));
    }

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

        var tallyDecryption = TallyDecryptions[tallyId];

        if (!tallyDecryption.CanDecrypt(Tallies[tallyId]))
        {
            return new DecryptionResult(tallyId, "Tally decryption is not valid");
        }

        return new DecryptionResult(tallyId);
    }

    /// <summary>
    /// Decrypt the tally
    /// </summary>
    public DecryptionResult Decrypt(string tallyId)
    {
        var canDecrypt = CanDecrypt(tallyId);
        if (!canDecrypt)
        {
            return canDecrypt;
        }

        var tallyDecryption = TallyDecryptions[tallyId];
        return tallyDecryption.Decrypt();
    }

    /// <summary>
    /// Submit a single tally share
    /// </summary>
    public void SubmitShare(
        CiphertextDecryptionTallyShare tallyShare)
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
    public void SubmitShare(
        CiphertextDecryptionBallotShare ballotShare,
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
        Tuple<CiphertextDecryptionTallyShare, Dictionary<string, CiphertextDecryptionBallotShare>> shares,
        List<CiphertextBallot> ballots
         )
    {
        SubmitShare(shares.Item1);
        SubmitShares(shares.Item2.Values
            .Select(i => new CiphertextDecryptionBallotShare(i)).ToList(),
            ballots.Select(i => new CiphertextBallot(i)).ToList());
    }

    /// <summary>
    /// Submit a list of tally shares and a list of ballot shares
    /// </summary>
    public void SubmitShares(
        CiphertextDecryptionTallyShare tallyShare,
        List<CiphertextDecryptionBallotShare> ballotShares,
        List<CiphertextBallot> ballots)
    {
        SubmitShare(tallyShare);
        SubmitShares(ballotShares, ballots);
    }

    /// <summary>
    /// Submit a list of ballot shares
    /// </summary>
    public void SubmitShares(
        List<CiphertextDecryptionBallotShare> ballotShares,
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

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();
        foreach (var tallyDecryption in TallyDecryptions.Values)
        {
            tallyDecryption.Dispose();
        }
        foreach (var guardian in Guardians.Values)
        {
            guardian.Dispose();
        }
        foreach (var tally in Tallies.Values)
        {
            tally.Dispose();
        }
    }

    private void EnsureCiphertextDecryptionTally(CiphertextDecryptionTallyShare share)
    {
        if (!Tallies.ContainsKey(share.TallyId))
        {
            throw new ArgumentException("Tally does not exist");
        }

        if (!Guardians.ContainsKey(share.GuardianId))
        {
            throw new ArgumentException("Guardian does not exist");
        }

        if (!TallyDecryptions.ContainsKey(share.TallyId))
        {
            TallyDecryptions.Add(
                share.TallyId,
                new CiphertextDecryptionTally(Tallies[share.TallyId]));
        }
    }
}
