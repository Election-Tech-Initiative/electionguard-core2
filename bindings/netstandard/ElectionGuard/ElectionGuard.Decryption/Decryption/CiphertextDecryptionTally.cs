using ElectionGuard.Decryption.Tally;
using ElectionGuard.ElectionSetup;
using ElectionGuard.ElectionSetup.Extensions;
using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.Decryption.Decryption;

/// <summary>
/// A container for the decryption state of a single tally. This object is designed to be hydrated from
/// a database and used by the Decryption Mediator to coordinate the decryption of a tally.
/// </summary>
public record class CiphertextDecryptionTally : DisposableRecordBase
{
    private readonly CiphertextTally _tally;

    public string TallyId => _tally.TallyId;

    // the guardians participating in this decryption
    private readonly Dictionary<string, ElectionPublicKey> _guardians = new();

    // ballots which will be individually decrypted.
    // spoiled ballots in most cases but sometimes individual ballots from the record can be decrypted
    // key is ballotid
    private readonly Dictionary<string, CiphertextBallot> _ballots = new();

    // key is guardianid
    private readonly Dictionary<string, CiphertextDecryptionTallyShare> _tallyShares = new();

    // key is ballotid
    private readonly Dictionary<string, CiphertextDecryptionBallot> _ballotShares = new();

    public CiphertextDecryptionTally(CiphertextTally tally)
    {
        _tally = tally;
    }

    public CiphertextDecryptionTally(
        CiphertextTally tally,
        List<ElectionPublicKey> guardians,
        List<CiphertextBallot> ballots)
    {
        _tally = tally;

        foreach (var guardian in guardians)
        {
            _guardians.Add(guardian.OwnerId, guardian);
        }

        foreach (var ballot in ballots)
        {
            AddBallot(ballot);
        }
    }

    /// <summary>
    /// add a tally share to the decryption tally
    /// </summary>
    public void AddTallyShare(
        ElectionPublicKey guardian,
        CiphertextDecryptionTallyShare tallyShare)
    {
        if (!tallyShare.IsValid(_tally, guardian))
        {
            throw new ArgumentException("Invalid tally share");
        }

        AddGuardian(guardian);
        AddShare(tallyShare);
    }

    /// <summary>
    /// add a ballot share to the decryption tally
    /// </summary>
    public void AddBallotShare(
        ElectionPublicKey guardian,
        CiphertextDecryptionBallotShare ballotShare,
        CiphertextBallot ballot)
    {
        if (!_tally.HasBallot(ballot.ObjectId))
        {
            throw new ArgumentException("Tally does not contain ballot");
        }

        if (!ballotShare.IsValid(ballot, guardian, _tally.Context.CryptoExtendedBaseHash))
        {
            throw new ArgumentException("Invalid ballot share");
        }

        AddGuardian(guardian);
        AddBallot(ballot);
        AddShare(ballotShare);
    }

    /// <summary>
    /// determine if the tally can be decrypted
    /// </summary>
    public bool CanDecrypt(CiphertextTally tally)
    {
        // There are not enough guardians to decrypt the tally
        if (_guardians.Count < (int)tally.Context.Quorum)
        {
            return false;
        }

        // some guardian shares have not been submitted, or there are too many
        if (_guardians.Count != _tallyShares.Count)
        {
            return false;
        }

        // check that all the shares are valid
        foreach (var (guardian, share) in GetGuardianShares())
        {
            if (!share.IsValid(tally, guardian))
            {
                return false;
            }
        }

        // check that the ballot shares are valid
        foreach (var (ballotId, share) in _ballotShares)
        {
            if (!share.IsValid(_ballots[ballotId], _tally.Context.CryptoExtendedBaseHash))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// decrypt the tally.
    /// </summary>
    public DecryptionResult Decrypt(CiphertextTally tally, bool skipValidation = false)
    {
        if (!skipValidation && !CanDecrypt(tally))
        {
            return new DecryptionResult("Tally is not valid");
        }

        var guardianShares = GetGuardianShares();
        var lagrangeCoefficients = guardianShares.ComputeLagrangeCoefficients();

        var plaintextTally = tally.Decrypt(
            guardianShares, lagrangeCoefficients, skipValidation);
        var plaintextBallots = _ballots.Decrypt(
            _ballotShares, lagrangeCoefficients, tally, skipValidation);

        return new DecryptionResult(tally.TallyId, plaintextTally, plaintextBallots);
    }

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();
        _tally.Dispose();
        _guardians.Dispose();
        _ballots.Dispose();
        foreach (var share in _tallyShares.Values)
        {
            share.Dispose();
        }
        foreach (var share in _ballotShares.Values)
        {
            share.Dispose();
        }
    }

    private void AddBallot(CiphertextBallot ballot)
    {
        if (!_ballots.ContainsKey(ballot.ObjectId))
        {
            _ballots.Add(ballot.ObjectId, ballot);
        }
    }

    private void AddGuardian(ElectionPublicKey guardian)
    {
        if (!_guardians.ContainsKey(guardian.OwnerId))
        {
            _guardians.Add(guardian.OwnerId, guardian);
        }
    }

    private void AddShare(CiphertextDecryptionTallyShare tallyshare)
    {
        if (!_tallyShares.ContainsKey(tallyshare.GuardianId))
        {
            _tallyShares.Add(tallyshare.GuardianId, tallyshare);
        }
    }

    private void AddShare(CiphertextDecryptionBallotShare ballotShare)
    {
        if (!_ballotShares.ContainsKey(ballotShare.BallotId))
        {
            _ballotShares.Add(
                ballotShare.BallotId,
                new CiphertextDecryptionBallot(
                    ballotShare, _guardians[ballotShare.GuardianId]));
            return;
        }

        _ballotShares[ballotShare.BallotId].AddShare(
            ballotShare, _guardians[ballotShare.GuardianId]);
    }

    private List<Tuple<ElectionPublicKey, CiphertextDecryptionTallyShare>> GetGuardianShares()
    {
        var guardianShares = new List<Tuple<ElectionPublicKey, CiphertextDecryptionTallyShare>>();
        foreach (var guardian in _guardians.Values)
        {
            var share = _tallyShares[guardian.OwnerId];
            guardianShares.Add(new Tuple<ElectionPublicKey, CiphertextDecryptionTallyShare>(guardian, share));
        }
        return guardianShares;
    }


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
