namespace ElectionGuard.UI.Services;

public class TallyStateMachine : ITallyStateMachine
{
    private TallyRecord? _tally = default;

    private AuthenticationService _authenticationService;
    private readonly ChallengeResponseService _challengeResponseService;
    private readonly DecryptionShareService _decryptionShareService;
    private readonly TallyJoinedService _tallyJoinedService;
    private readonly TallyService _tallyService;
    private TallyManager _tallyManager;

    public async Task Run(TallyRecord tally)
    {
    }

    public TallyStateMachine(
        AuthenticationService authenticationService,
        ChallengeResponseService challengeResponseService,
        DecryptionShareService decryptionShareService,
        TallyService tallyService,
        TallyManager tallyManager,
        TallyJoinedService tallyJoinedService)
    {
        _authenticationService = authenticationService;
        _challengeResponseService = challengeResponseService;
        _decryptionShareService = decryptionShareService;
        _tallyService = tallyService;
        _tallyJoinedService = tallyJoinedService;
        _tallyManager = tallyManager;
    }

    private async Task<bool> ShouldStartTally(TallyRecord tally)
    {
        if (! _authenticationService.IsAdmin || tally.State != TallyState.PendingGuardiansJoin)
        {
            return false;
        }

        var joinedGuardians = await _tallyJoinedService.GetCountByTallyJoinedAsync(tally.TallyId);

        return joinedGuardians >= tally.Quorum;
    }

    private async Task<bool> ShouldAcccumulateTally(TallyRecord tally)
    {
        return await Task.FromResult(_authenticationService.IsAdmin && tally.State == TallyState.AdminStartsTally);
    }

    private async Task<bool> ShouldDecryptShares(TallyRecord tally)
    {
        if (_authenticationService.IsAdmin || tally.State != TallyState.PendingGuardianDecryptShares)
        {
            return false;
        }

        return !await _decryptionShareService.GetExistsByTallyAsync(tally.TallyId, _authenticationService.UserName ?? string.Empty);
    }

    private async Task<bool> ShouldGenerateChallenge(TallyRecord tally)
    {
        if (!_authenticationService.IsAdmin || tally.State != TallyState.AdminGenerateChallenge)
        {
            return false;
        }

        var joinedGuardians = await _tallyJoinedService.GetCountByTallyJoinedAsync(tally.TallyId);
        var decryptionShares = await _decryptionShareService.GetCountByTallyAsync(tally.TallyId);

        return decryptionShares == joinedGuardians;
    }

    private async Task<bool> ShouldRespondChallenge(TallyRecord tally)
    {
        if (_authenticationService.IsAdmin || tally.State != TallyState.PendingGuardianRespondChallenge)
        {
            return false;
        }
        // Challenge created for joined guardian, guardian has not responded yet
        return ! await _challengeResponseService.GetExistsByTallyAsync(
            tally.TallyId,
            _authenticationService.UserName ?? string.Empty);
    }

    private async Task<bool> ShouldVerifyChallenge(TallyRecord tally)
    {
        return await Task.FromResult(_authenticationService.IsAdmin && tally.State != TallyState.AdminVerifyChallenge);
    }
}
