using ElectionGuard.Decryption.Tally;
using ElectionGuard.ElectionSetup;
using ElectionGuard.ElectionSetup.Extensions;
using ElectionGuard.Guardians;

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
    // challenged ballots in most cases but sometimes individual ballots from the record can be decrypted
    // key is ballotid
    private readonly Dictionary<string, CiphertextBallot> _challengedBallots = new();

    // ballots that will not be decrypted
    private readonly Dictionary<string, CiphertextBallot> _spoiledBallots = new();

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
        _tally = new(tally);

        foreach (var guardian in guardians)
        {
            _guardians.Add(guardian.OwnerId, guardian);
        }

        foreach (var ballot in ballots)
        {
            AddBallot(ballot);
        }
    }

    public void AddBallot(CiphertextBallot ballot)
    {
        if (ballot.IsChallenged)
        {
            AddChallengedBallot(ballot);
        }
        else
        {
            AddSpoiledBallot(ballot);
        }
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

        if (ballot.IsSpoiled)
        {
            throw new ArgumentException("Cannot decrypt spoiled ballot");
        }

        if (!ballotShare.IsValid(
            ballot, guardian, _tally.Context.CryptoExtendedBaseHash))
        {
            throw new ArgumentException("Invalid ballot share");
        }

        AddGuardian(guardian);
        AddChallengedBallot(ballot);
        AddShare(ballotShare);
    }

    public void AddSpoiledBallot(CiphertextBallot ballot)
    {
        if (!_tally.HasBallot(ballot.ObjectId))
        {
            throw new ArgumentException("Tally does not contain ballot");
        }

        if (!ballot.IsSpoiled)
        {
            throw new ArgumentException("Cannot add unspoiled ballot");
        }

        if (!_spoiledBallots.ContainsKey(ballot.ObjectId))
        {
            _spoiledBallots.Add(ballot.ObjectId, ballot);
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
    /// determine if the tally can be decrypted
    /// </summary>
    public bool CanDecrypt(CiphertextTally tally)
    {
        // There are not enough guardians to decrypt the tally
        if (_guardians.Count < (int)tally.Context.Quorum)
        {
            return false;
        }

        // some guardian shares have not been submitted or there are too many
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
            if (!_tally.ChallengedBallotIds.Contains(ballotId))
            {
                // tally doesnt include the ballot
                return false;
            }

            var challengedBallot = _challengedBallots[ballotId];
            if (!share.IsValid(
                challengedBallot, _tally.Context))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// decrypt the tally.
    /// </summary>
    public DecryptionResult Decrypt(bool skipValidation = false)
    {
        if (!skipValidation && !CanDecrypt(_tally))
        {
            return new DecryptionResult("Tally is not valid");
        }

        var guardianShares = GetGuardianShares();
        var lagrangeCoefficients = guardianShares.ComputeLagrangeCoefficients();

        var plaintextTally = _tally.Decrypt(
            guardianShares, lagrangeCoefficients, skipValidation);
        var plaintextBallots = _challengedBallots.Decrypt(
            _ballotShares, lagrangeCoefficients, _tally, skipValidation);
        var spoiledBallots = _spoiledBallots.Values.ToList();

        return new DecryptionResult(
            _tally.TallyId, plaintextTally, plaintextBallots, spoiledBallots);
    }

    public DecryptionResult DecryptBallot(string ballotId, bool skipValidation = false)
    {
        var ballot = _challengedBallots[ballotId];
        var ballotShares = _ballotShares[ballotId];
        var guardianShares = GetGuardianShares();
        var lagrangeCoefficients = guardianShares.ComputeLagrangeCoefficients();
        return ballot.Decrypt(ballotShares, lagrangeCoefficients, _tally, skipValidation);
    }

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();
        _tally?.Dispose();
        _guardians?.Dispose();
        _challengedBallots?.Dispose();
        _spoiledBallots?.Dispose();
        foreach (var share in _tallyShares.Values)
        {
            share?.Dispose();
        }
        foreach (var share in _ballotShares.Values)
        {
            share?.Dispose();
        }
    }

    private void AddChallengedBallot(CiphertextBallot ballot)
    {
        if (!_challengedBallots.ContainsKey(ballot.ObjectId))
        {
            _challengedBallots.Add(ballot.ObjectId, ballot);
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
}
