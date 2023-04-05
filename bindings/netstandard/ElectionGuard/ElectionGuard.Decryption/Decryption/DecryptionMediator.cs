using System;
using System.Collections.Generic;
using System.Linq;
using ElectionGuard;
using ElectionGuard.UI.Lib.Models;
using ElectionGuard.Decryption.Tally;

namespace ElectionGuard.Decryption.Decryption;

public enum DecryptionState
{
    DoesNotExist = 0,
    PendingGuardians = 1,
    PendingAdminAnnounce = 2,
    PendingGuardianBackups = 3,
    PendingAdminToShareBackups = 4,
    PendingGuardiansVerifyBackups = 5,
    PendingAdminToPublishJointKey = 6,
    Complete = 7
}

// a container for the decryption state of a single tally
public class TallyDecryption
{
    public string TallyId { get; init; }

    // the guardians participating in this decryption
    public Dictionary<string, ElectionPublicKey> Guardians { get; init; } = new Dictionary<string, ElectionPublicKey>();

    // key is the guardian id
    // public Dictionary<string, ElementModQ> LaGrangeCoefficients { get; private set; } = new Dictionary<string, ElementModQ>();

    // key is guardianid
    public Dictionary<string, CiphertextDecryptionTallyShare> TallyShares { get; init; } = new Dictionary<string, CiphertextDecryptionTallyShare>();

    // key is ballotid
    public Dictionary<string, CiphertextDecryptionBallotShares> BallotShares { get; init; } = new Dictionary<string, CiphertextDecryptionBallotShares>();

    public TallyDecryption(string tallyId)
    {
        TallyId = tallyId;
    }

    public TallyDecryption(CiphertextTally tally)
    {
        TallyId = tally.TallyId;
    }

    public bool IsValid(CiphertextTally tally)
    {
        // There are not enough guardains to decrypt the tally
        if (Guardians.Count < (int)tally.Context.Quorum)
        {
            return false;
        }

        // some guardian shares have not been submitted, or there are too many
        if (Guardians.Count != TallyShares.Count)
        {
            return false;
        }

        // check that all the shares are valid
        foreach (var share in TallyShares.Values)
        {
            if (!share.IsValid(tally))
            {
                return false;
            }
        }

        // TODO: more checks, like ballot shares

        return true;
    }

    public PlaintextTally Decrypt(CiphertextTally tally, bool skipValidation = false)
    {
        if (!skipValidation && !IsValid(tally))
        {
            throw new ArgumentException("Tally is not valid");
        }

        //var lagrangeCoefficients = ComputeLagrangeCoefficients();

        var guardianShares = GetGuardianShares();

        return tally.Decrypt(guardianShares, tally.Context.CryptoExtendedBaseHash, skipValidation);
    }

    private List<Tuple<ElectionPublicKey, CiphertextDecryptionTallyShare>> GetGuardianShares()
    {
        var guardianShares = new List<Tuple<ElectionPublicKey, CiphertextDecryptionTallyShare>>();
        foreach (var guardian in Guardians.Values)
        {
            var share = TallyShares[guardian.OwnerId];
            guardianShares.Add(new Tuple<ElectionPublicKey, CiphertextDecryptionTallyShare>(guardian, share));
        }
        return guardianShares;
    }

    // private List<Tuple<ElectionPublicKey, CiphertextDecryptionBallotShare>> GetGuardianBallotShares(string ballotId)
    // {
    //     var guardianShares = new List<Tuple<ElectionPublicKey, CiphertextDecryptionBallotShare>>();
    //     foreach (var guardian in Guardians.Values)
    //     {
    //         var share = BallotShares[ballotId].BallotShares[guardian.OwnerId];
    //         guardianShares.Add(new Tuple<ElectionPublicKey, CiphertextDecryptionBallotShare>(guardian, share));
    //     }
    //     return guardianShares;
    // }

    // private Dictionary<string, ElementModQ> ComputeLagrangeCoefficients()
    // {
    //     var lagrangeCoefficients = new Dictionary<string, ElementModQ>();
    //     foreach (var guardian in Guardians.Values)
    //     {
    //         var otherSequenceOrders = Guardians.Values
    //             .Where(i => i.OwnerId != guardian.OwnerId)
    //             .Select(x => x.SequenceOrder).ToList();
    //         var lagrangeCoefficient = Polynomial.Interpolate(
    //             guardian.SequenceOrder, otherSequenceOrders
    //             );
    //         lagrangeCoefficients.Add(guardian.OwnerId, lagrangeCoefficient);
    //     }
    //     return lagrangeCoefficients;
    // }

    // private Dictionary<string, ElementModQ> ComputeLagrangeCoefficients(
    //     List<Tuple<ElectionPublicKey, CiphertextDecryptionTallyShare>> guardianShares)
    // {
    //     var lagrangeCoefficients = new Dictionary<string, ElementModQ>();
    //     foreach (var guardian in Guardians.Values)
    //     {
    //         var otherSequenceOrders = guardianShares
    //             .Where(i => i.Item1.OwnerId != guardian.OwnerId)
    //             .Select(x => x.Item1.SequenceOrder).ToList();
    //         var lagrangeCoefficient = Polynomial.Interpolate(
    //             guardian.SequenceOrder, otherSequenceOrders
    //             );
    //         lagrangeCoefficients.Add(guardian.OwnerId, lagrangeCoefficient);
    //     }
    //     return lagrangeCoefficients;
    // }

    // public PlaintextBallot DecryptBallot(CiphertextBallot ballot, bool skipValidation = false)
    // {
    //     if (!skipValidation && !IsValid(ballot.Tally))
    //     {
    //         throw new ArgumentException("Tally is not valid");
    //     }

    //     var guardianShares = GetGuardianBallotShares(ballot.BallotId);

    //     return ballot.Decrypt(guardianShares, ballot.Tally.Context.CryptoExtendedBaseHash, skipValidation);
    // }

}

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
}
