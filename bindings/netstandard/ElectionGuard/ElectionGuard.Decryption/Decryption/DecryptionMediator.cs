using System;
using System.Collections.Generic;
using System.Linq;
using ElectionGuard;
using ElectionGuard.UI.Lib.Models;
using ElectionGuard.Decryption.Tally;

namespace ElectionGuard.Decryption.Decryption;


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

        Tallies.Add(tally.TallyId, tally);
        foreach (var guardian in guardians)
        {
            Guardians.Add(guardian.OwnerId, guardian);
        }
    }

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

        if (!tallyDecryption.IsValid(Tallies[tallyId]))
        {
            return new DecryptionResult(tallyId, "Tally decryption is not valid");
        }

        return new DecryptionResult(tallyId);
    }

    public DecryptionResult Decrypt(string tallyId)
    {
        var canDecrypt = CanDecrypt(tallyId);
        if (!canDecrypt)
        {
            return canDecrypt;
        }

        var tally = Tallies[tallyId];
        var tallyDecryption = TallyDecryptions[tallyId];
        return tallyDecryption.Decrypt(tally);
    }

    public void SubmitShare(
        CiphertextDecryptionTallyShare tallyShare)
    {
        // TODO: validate the share
        AddTallyDecryption(tallyShare);
        var tallyDecryption = TallyDecryptions[tallyShare.TallyId];
        tallyDecryption.AddShare(Guardians[tallyShare.GuardianId], tallyShare);
    }

    public void SubmitShare(CiphertextDecryptionBallotShare ballotShare)
    {
        // TODO: validate the share
        AddTallyDecryption(ballotShare);
        var tallyDecryption = TallyDecryptions[ballotShare.TallyId];
        tallyDecryption.AddShare(Guardians[ballotShare.GuardianId], ballotShare);
    }

    public void SubmitShares(
        Tuple<CiphertextDecryptionTallyShare, Dictionary<string, CiphertextDecryptionBallotShare>> shares
         )
    {
        SubmitShare(shares.Item1);
        SubmitShares(shares.Item2.Values.ToList());
    }

    public void SubmitShares(
        CiphertextDecryptionTallyShare tallyShare,
        List<CiphertextDecryptionBallotShare> ballotShares)
    {
        SubmitShare(tallyShare);
        SubmitShares(ballotShares);
    }

    public void SubmitShares(List<CiphertextDecryptionBallotShare> ballotShares)
    {
        // TODO: validate the share
        AddTallyDecryption(ballotShares.First());
        var tallyDecryption = TallyDecryptions[ballotShares.First().TallyId];
        var guardian = Guardians[ballotShares.First().GuardianId];
        foreach (var ballotShare in ballotShares)
        {
            tallyDecryption.AddShare(guardian, ballotShare);
        }
    }

    private void AddTallyDecryption(CiphertextDecryptionTallyShare share)
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
            TallyDecryptions.Add(share.TallyId, new CiphertextDecryptionTally(share.TallyId));
        }
    }
}
