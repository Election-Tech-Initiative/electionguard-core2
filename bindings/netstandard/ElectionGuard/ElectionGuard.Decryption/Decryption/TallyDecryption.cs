using ElectionGuard.Decryption.Tally;
using ElectionGuard.ElectionSetup;
using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.Decryption.Decryption;

// a container for the decryption state of a single tally
public record class TallyDecryption : DisposableRecordBase
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
