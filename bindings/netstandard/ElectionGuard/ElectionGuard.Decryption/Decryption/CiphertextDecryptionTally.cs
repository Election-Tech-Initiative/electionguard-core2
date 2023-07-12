using ElectionGuard.Decryption.Accumulation;
using ElectionGuard.Decryption.Challenge;
using ElectionGuard.Decryption.ChallengeResponse;
using ElectionGuard.Decryption.Extensions;
using ElectionGuard.Decryption.Shares;
using ElectionGuard.Decryption.Tally;
using ElectionGuard.ElectionSetup;
using ElectionGuard.ElectionSetup.Extensions;
using ElectionGuard.Extensions;
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

    #region computed and cached values

    // key is guardianid
    private Dictionary<string, TallyShare> _tallyShares { get; init; } = new();

    // key is ballotid
    private Dictionary<string, CiphertextDecryptionBallot> _ballotShares { get; init; } = new();

    // key is guardianid
    private Dictionary<string, GuardianChallenge> _challenges { get; init; } = new();

    // key is guardianid
    private Dictionary<string, GuardianChallengeResponse> _challengeResponses { get; init; } = new();

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
        AddBallots(ballots);

        DecryptionState = DecryptionState.PendingGuardianShares;
    }

    #region Misc Admin

    public void AddBallots(List<CiphertextBallot> ballots)
    {
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

    #endregion

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

    // TODO: async version
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

        // state transition
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
                _tally.TallyId,
                _ballotShares,
                lagrangeCoefficients,
                _tally.Context.CryptoExtendedBaseHash,
                skipValidation);
    }

    #endregion

    #region Challenge

    public void LoadChallenge(string guardianId, GuardianChallenge challenge)
    {
        _challenges.Add(guardianId, challenge);
        if(_challenges.Count == _guardians.Count)
        {
            // state transition
            DecryptionState = DecryptionState.PendingGuardianChallengeResponses;
        }
    }


    /// <summary>
    /// Create a challenge for each available guardian that includes the tally and ballot challenges
    /// which are offset by the lagrange coefficients for the guardian.
    ///
    /// At this point in the protocol the administrator commits to the quorum of guardains by accumulating
    /// the tally and ballot shares for all guardians that have submitted their shares.
    /// from this point on, all guardians who have submitted shares must complete the protocol.
    /// It is possible to restart from this step in certain circumstances such as when a guardian
    /// is removed from the quorum, or a new guardian is added. 
    /// (to do this you must reset the state manually and re-run AccumulateShares)
    /// </summary>
    public Dictionary<string, GuardianChallenge> CreateChallenge(bool skipValidation = false)
    {
        if (DecryptionState != DecryptionState.PendingAdminChallenge)
        {
            _ = AccumulateShares(skipValidation);
        }

        var tallyChallenges = CreateTallyChallenge();
        var ballotChallenges = CreateBallotChallenges();

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

        // state transition
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
            _accumulatedBallots, _tally.TallyId, lagrangeCoefficients, _tally.Context);

        return challenges;

    }

    #endregion

    #region Challenge Response

    public bool CanValidateResponses()
    {
        if (_challengeResponses.Count == _challenges.Count)
        {
            return true;
        }

        return false;
    }

    public void SubmitChallengeResponse(GuardianChallengeResponse response)
    {
        if (DecryptionState != DecryptionState.PendingGuardianChallengeResponses)
        {
            throw new ArgumentException("Cannot submit challenge response while state is " + DecryptionState);
        }

        if (!_challenges.ContainsKey(response.GuardianId))
        {
            throw new ArgumentException("Guardian challenge does not exist");
        }

        _challengeResponses.Add(response.GuardianId, response);

        // state transition
        if (CanValidateResponses())
        {
            DecryptionState = DecryptionState.PendingAdminValidateResponses;
        }
    }

    // TODO: return a result
    // TODO: async
    public bool ValidateChallengeResponses()
    {
        if (!CanValidateResponses())
        {
            throw new ArgumentException("Cannot validate challenge responses");
        }

        var challengeBallots = _challengedBallots.Values.ToList();

        foreach (var (guardianId, response) in _challengeResponses)
        {
            var share = GetShare(guardianId);

            // TODO: return the accumulated responses so that we can make the cp proofs

            if (!response.IsValid(
                _tally,
                challengeBallots,
                share,
                _challenges[guardianId]))
            {
                Console.WriteLine("Invalid challenge response for guardian " + guardianId);
                return false;
            }
        }

        // state transition
        DecryptionState = DecryptionState.PendingAdminComputeProofs;

        return true;
    }

    // TODO: async
    public void ComputeDecryptionProofs(bool skipValidation = false)
    {
        if (DecryptionState != DecryptionState.PendingAdminComputeProofs)
        {
            throw new ArgumentException("Cannot create proofs while state is " + DecryptionState);
        }

        // compute the CP proofs and add them to the accumulated values
        // so that they can be used as part of decryption

        _accumulatedTally!.AddProofs(
            _tally,
            _challengeResponses.Values.ToList(),
            skipValidation
        );

        _accumulatedBallots!.AddProofs(
            _challengedBallots.Values.ToList(),
            _tally.Context,
            _challengeResponses.Values.ToList(),
            skipValidation
        );

        // state transition
        DecryptionState = DecryptionState.PendingAdminDecryptShares;
    }

    #endregion

    # region Decrypt

    /// <summary>
    /// determine if the tally can be decrypted
    /// </summary>
    public bool CanDecrypt(CiphertextTally tally)
    {
        // TODO: break up these checks into their appropriate locations

        if (DecryptionState != DecryptionState.PendingAdminDecryptShares)
        {
            throw new ArgumentException("Cannot decrypt while state is " + DecryptionState);
        }

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
    // TODO: async
    public DecryptionResult Decrypt(bool regenerate = false, bool skipValidation = false)
    {
        // if we already have a result, return it
        if (regenerate && _decryptionResult != null)
        {
            return _decryptionResult;
        }

        // try to compute the proofs if they ahve not been computed already
        if (DecryptionState == DecryptionState.PendingAdminComputeProofs)
        {
            ComputeDecryptionProofs(skipValidation);
        }

        if (!skipValidation && !CanDecrypt(_tally))
        {
            return new DecryptionResult("Tally is not valid");
        }

        var guardianShares = GetGuardianShares();
        var lagrangeCoefficients = guardianShares.ComputeLagrangeCoefficients();

        // decrypt
        var plaintextTally = _tally.Decrypt(
            guardianShares, lagrangeCoefficients, skipValidation);
        var plaintextBallots = _challengedBallots.Decrypt(
            _ballotShares, lagrangeCoefficients, _tally, skipValidation);
        var spoiledBallots = _spoiledBallots.Values.ToList();

        // construct the decryption result
        var result = new DecryptionResult(
            _tally.TallyId, plaintextTally, plaintextBallots, spoiledBallots);

        DecryptionState = DecryptionState.PendingAdminPublishResults;

        _decryptionResult = result;

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

    // TODO: implement verification methods 
    // (these are the methods that a 3rd aprty verifier would implement)

    #endregion

    #region Publish

    // TODO: implement publish methods

    #endregion

    protected override void DisposeManaged()
    {
        base.DisposeManaged();
        _tally?.Dispose();
        _guardians?.Dispose();
        _challengedBallots?.Dispose();
        _spoiledBallots?.Dispose();

        _tallyShares?.Dispose();
        _ballotShares?.Dispose();

        _challenges?.Dispose();
        _challengeResponses?.Dispose();

        _accumulatedTally?.Dispose();
        _accumulatedBallots?.Dispose();
        _decryptionResult?.Dispose();
    }

    #region Private Methods

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
            _guardians.Add(guardian.GuardianId, new(guardian));
        }
    }

    private void AddShare(TallyShare tallyshare)
    {
        if (!_tallyShares.ContainsKey(tallyshare.GuardianId))
        {
            _tallyShares.Add(tallyshare.GuardianId, new(tallyshare));
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
            guardianShares.Add(
                new Tuple<ElectionPublicKey, TallyShare>(guardian, share));
        }
        return guardianShares;
    }

    #endregion
}
