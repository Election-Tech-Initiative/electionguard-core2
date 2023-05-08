using ElectionGuard.Decryption.Accumulation;
using ElectionGuard.Decryption.Challenge;
using ElectionGuard.Decryption.Extensions;
using ElectionGuard.Decryption.Shares;
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

    public DecryptionState DecryptionState { get; set; } = DecryptionState.DoesNotExist;

    #region exernally provided values

    // the guardians participating in this decryption
    private readonly Dictionary<string, ElectionPublicKey> _guardians = new();

    // ballots which will be individually decrypted.
    // challenged ballots in most cases but sometimes 
    // individual ballots from the record can be decrypted
    // key is ballotid
    private readonly Dictionary<string, CiphertextBallot> _challengedBallots = new();

    // ballots that will not be decrypted
    private readonly Dictionary<string, CiphertextBallot> _spoiledBallots = new();

    #endregion

    #region computed and cahced values
    // key is guardianid
    private Dictionary<string, TallyShare> _tallyShares { get; init; } = new();

    // key is ballotid
    private Dictionary<string, CiphertextDecryptionBallot> _ballotShares { get; init; } = new();

    // key is guardianid
    private Dictionary<string, GuardianChallenge> _challenges { get; init; } = new();

    #endregion

    #region computed values (dont have to be serialized)

    private AccumulatedTally? _accumulatedTally;

    private List<AccumulatedBallot>? _accumulatedBallots;

    private DecryptionResult? _decryptionResult;

    #endregion

    public CiphertextDecryptionTally(CiphertextTally tally)
    {
        _tally = tally;
        DecryptionState = DecryptionState.PendingGuardianShares;
    }

    public CiphertextDecryptionTally(
        CiphertextTally tally,
        List<ElectionPublicKey> guardians,
        List<CiphertextBallot> ballots)
    {
        _tally = new(tally);

        foreach (var guardian in guardians)
        {
            _guardians.Add(guardian.GuardianId, guardian);
        }

        foreach (var ballot in ballots)
        {
            AddBallot(ballot);
        }
        DecryptionState = DecryptionState.PendingGuardianShares;
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

        // TODO: check that all ballots have been added
    }

    // add a spoiled ballot which will not be decrypted
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

        // TODO: check that all spoiled ballots are received
    }

    #region Submit Shares

    /// <summary>
    /// add a ballot share to the decryption tally
    /// </summary>
    public void AddBallotShare(
        ElectionPublicKey guardian,
        BallotShare ballotShare,
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

        // TODO: check if all shares received against the tally
    }

    /// <summary>
    /// add a tally share to the decryption tally
    /// </summary>
    public void AddTallyShare(
        ElectionPublicKey guardian,
        TallyShare tallyShare)
    {
        if (!tallyShare.IsValid(_tally, guardian))
        {
            throw new ArgumentException("Invalid tally share");
        }

        AddGuardian(guardian);
        AddShare(tallyShare);
    }

    public GuardianShare GetShare(string guardianId)
    {
        if (!_guardians.ContainsKey(guardianId))
        {
            throw new ArgumentException("Tally does not contain guardian");
        }

        if (!_tallyShares.ContainsKey(guardianId))
        {
            throw new ArgumentException("Tally does not contain share");
        }

        // get ballot shares if any, it's possible that a guardian
        // has not submitted any ballot shares
        // if this api is queried before all shares are submitted
        var ballotShares = _ballotShares.Values
            .Where(x => x.GetShare(guardianId) != null)
            .Select(x => x.GetShare(guardianId)!)
            .ToList();

        return new GuardianShare(
            _guardians[guardianId],
            _tallyShares[guardianId],
            ballotShares ?? new List<BallotShare>()
            );
    }

    #endregion

    #region Accumulate Shares

    public bool CanAccumulate()
    {
        // There are enough guardians to decrypt the tally
        if (_tallyShares.Count >= (int)_tally.Context.Quorum)
        {
            return true;
        }
        return false;
    }

    public Tuple<AccumulatedTally, List<AccumulatedBallot>> AccumulateShares(
        bool skipValidation = false)
    {
        if (!CanAccumulate())
        {
            throw new ArgumentException("Cannot accumulate shares");
        }

        _accumulatedTally = AccumulateTallyShares(skipValidation);
        _accumulatedBallots = AccumulateBallotShares(skipValidation);

        var accumulationResult = new Tuple<AccumulatedTally, List<AccumulatedBallot>>(
            _accumulatedTally, _accumulatedBallots);

        DecryptionState = DecryptionState.PendingAdminChallenge;

        return accumulationResult;
    }

    /// <summary>
    /// accumulate the tally shares
    /// </summary>
    protected AccumulatedTally AccumulateTallyShares(bool skipValidation = false)
    {
        // TODO: do not recompute lagrange coefficients
        var guardianShares = GetGuardianShares();
        var lagrangeCoefficients = guardianShares.ComputeLagrangeCoefficients();

        return _tally.AccumulateShares(
            guardianShares, lagrangeCoefficients, skipValidation);
    }

    protected List<AccumulatedBallot> AccumulateBallotShares(bool skipValidation = false)
    {
        // TODO: do not recompute lagrange coefficients
        var guardianShares = GetGuardianShares();
        var lagrangeCoefficients = guardianShares.ComputeLagrangeCoefficients();

        return _challengedBallots.AccumulateShares(
                _ballotShares,
                lagrangeCoefficients,
                _tally.Context.CryptoExtendedBaseHash,
                skipValidation);
    }

    #endregion

    #region Challenge

    // create a challenge for each guardian that includes the tally and ballot challenges
    // which are offset by the lagrange coefficients for the guardian
    public Dictionary<string, GuardianChallenge> CreateChallenge(bool skipValidation = false)
    {
        if (DecryptionState != DecryptionState.PendingAdminChallenge)
        {
            _ = AccumulateShares(skipValidation);
        }

        var tallyChallenges = CreateTallyChallenge();
        var ballotChallenges = CreateBallotChallenges();

        Console.WriteLine($"Tally challenges: {tallyChallenges.Count}");
        Console.WriteLine($"Ballot challenges: {ballotChallenges?.Count}");

        // create guardian challenges for each guardian
        foreach (var (guardianId, guardian) in _guardians)
        {
            // get ballot challenges for guardian if any, it's possible that a guardian
            // has not submitted any ballot shares
            List<BallotChallenge>? ballotChallengesForGuardian = null;
            _ = (ballotChallenges?.TryGetValue(guardianId, out ballotChallengesForGuardian));

            _challenges.Add(guardianId, new GuardianChallenge(
                guardian,
                tallyChallenges[guardianId],
                ballotChallengesForGuardian));
        }

        DecryptionState = DecryptionState.PendingGuardianChallengeResponses;

        return _challenges;
    }

    // guardian scoped challenges for tally
    protected Dictionary<string, TallyChallenge> CreateTallyChallenge()
    {
        if (_accumulatedTally == null)
        {
            throw new ArgumentException("Tally has not been accumulated");
        }

        // TODO: do not recompute lagrange coefficients
        var guardianShares = GetGuardianShares();
        var lagrangeCoefficients = guardianShares.ComputeLagrangeCoefficients();

        var challenges = _tally.CreateChallenge(
            _accumulatedTally, lagrangeCoefficients);

        return challenges;
    }

    // guardian scoped challenges for ballots
    protected Dictionary<string, List<BallotChallenge>>? CreateBallotChallenges()
    {
        // TODO: async version

        if (_accumulatedBallots == null)
        {
            throw new ArgumentException("Ballots have not been accumulated");
        }

        if (_accumulatedBallots.Count == 0)
        {
            return null;
        }

        // TODO: do not recompute lagrange coefficients
        var guardianShares = GetGuardianShares();
        var lagrangeCoefficients = guardianShares.ComputeLagrangeCoefficients();

        var challenges = _challengedBallots.Values.ToList().CreateChallenges(
            _accumulatedBallots, lagrangeCoefficients, _tally.Context);

        return challenges;

    }

    #endregion

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

        // check that all of the commitments have been made

        // check that all of the challenges have been responded to

        // check that all of the challenge responses are valid



        return true;
    }

    /// <summary>
    /// decrypt the tally.
    /// </summary>
    public DecryptionResult Decrypt(bool regenerate = false, bool skipValidation = false)
    {
        if (regenerate && _decryptionResult != null)
        {
            return _decryptionResult;
        }

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

        // Create the decryption proofs

        var result = new DecryptionResult(
            _tally.TallyId, plaintextTally, plaintextBallots, spoiledBallots);

        DecryptionState = DecryptionState.PendingAdminPublishResults;

        return result;
    }

    public DecryptionResult DecryptBallot(string ballotId, bool skipValidation = false)
    {
        var ballot = _challengedBallots[ballotId];
        var ballotShares = _ballotShares[ballotId];
        var guardianShares = GetGuardianShares();
        var lagrangeCoefficients = guardianShares.ComputeLagrangeCoefficients();
        return ballot.Decrypt(ballotShares, lagrangeCoefficients, _tally, skipValidation);
    }

    #endregion

    #region Verify

    #endregion

    #region Publish

    #endregion

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
        if (!_guardians.ContainsKey(guardian.GuardianId))
        {
            _guardians.Add(guardian.GuardianId, guardian);
        }
    }

    private void AddShare(TallyShare tallyshare)
    {
        if (!_tallyShares.ContainsKey(tallyshare.GuardianId))
        {
            _tallyShares.Add(tallyshare.GuardianId, tallyshare);
        }
    }

    private void AddShare(BallotShare ballotShare)
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

    private List<Tuple<ElectionPublicKey, TallyShare>> GetGuardianShares()
    {
        var guardianShares = new List<Tuple<ElectionPublicKey, TallyShare>>();
        foreach (var guardian in _guardians.Values)
        {
            var share = _tallyShares[guardian.GuardianId];
            guardianShares.Add(new Tuple<ElectionPublicKey, TallyShare>(guardian, share));
        }
        return guardianShares;
    }
}
