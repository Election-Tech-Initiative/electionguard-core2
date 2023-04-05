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
    public Dictionary<string, TallyDecryption> TallyDecryptions { get; init; } = new Dictionary<string, TallyDecryption>();

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

    public bool CanDecrypt(string tallyId)
    {
        // TODO: implement
        return true;
    }

    public PlaintextTally Decrypt(string tallyId)
    {
        if (!Tallies.ContainsKey(tallyId))
        {
            throw new ArgumentException("Tally does not exist");
        }

        if (!TallyDecryptions.ContainsKey(tallyId))
        {
            throw new ArgumentException("Tally decryption does not exist");
        }

        var tallyDecryption = TallyDecryptions[tallyId];

        if (!tallyDecryption.IsValid(Tallies[tallyId]))
        {
            throw new ArgumentException("Tally decryption is not valid");
        }

        var tally = Tallies[tallyId];
        return tallyDecryption.Decrypt(tally);
    }

    // todo cleanup, more and better overrides
    public void SubmitShare(
        CiphertextDecryptionTallyShare tallyShare)
    {
        if (!Tallies.ContainsKey(tallyShare.TallyId))
        {
            throw new ArgumentException("Tally does not exist");
        }

        if (!Guardians.ContainsKey(tallyShare.GuardianId))
        {
            throw new ArgumentException("Guardian does not exist");
        }

        if (!TallyDecryptions.ContainsKey(tallyShare.TallyId))
        {
            TallyDecryptions.Add(tallyShare.TallyId, new TallyDecryption(tallyShare.TallyId));
        }

        var tallyDecryption = TallyDecryptions[tallyShare.TallyId];

        if (!tallyDecryption.Guardians.ContainsKey(tallyShare.GuardianId))
        {
            tallyDecryption.Guardians.Add(tallyShare.GuardianId, Guardians[tallyShare.GuardianId]);
        }

        if (!tallyDecryption.TallyShares.ContainsKey(tallyShare.GuardianId))
        {
            tallyDecryption.TallyShares.Add(tallyShare.GuardianId, tallyShare);
        }
    }
}
