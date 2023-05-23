using ElectionGuard.ElectionSetup;
using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.Services;

public class TallyStateMachine : ITallyStateMachine
{
    private TallyRecord _tally = new();
    private Dictionary<bool, List<StateMachineStep<TallyState>>> _steps = new();
    private bool _isRunning = false;

    private IAuthenticationService _authenticationService;
    private readonly ChallengeResponseService _challengeResponseService;
    private readonly DecryptionShareService _decryptionShareService;
    private readonly TallyJoinedService _tallyJoinedService;
    private readonly TallyService _tallyService;
    private TallyManager _tallyManager;

    public TallyStateMachine(
        IAuthenticationService authenticationService,
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
        GenerateGuardianSteps();
        GenerateAdminSteps();
    }

    public async Task Run(TallyRecord tally)
    {
        if (!_isRunning)
        {
            _tally = tally;

            var steps = _steps[_authenticationService.IsAdmin];
            var currentStep = steps.SingleOrDefault(s => s.State == _tally.State);
            if (currentStep is not null && await currentStep.ShouldRunStep())
            {
                _isRunning = true;
                try
                {
                    await currentStep.RunStep();
                }
                finally
                {
                    _isRunning = false;
                }
            }
        }
    }

    private async Task<bool> ShouldStartTally()
    {
        var joinedGuardians = await _tallyJoinedService.GetCountByTallyJoinedAsync(_tally.TallyId);
        return joinedGuardians >= _tally.Quorum;
    }

    private async Task<bool> AlwaysRun()
    {
        return await Task.FromResult(true);
    }

    #region Should Run
    private async Task<bool> ShouldDecryptShares()
    {
        return !await _decryptionShareService.GetExistsByTallyAsync(_tally.TallyId, _authenticationService.UserName ?? string.Empty);
    }

    private async Task<bool> ShouldGenerateChallenge()
    {
        var joinedGuardians = await _tallyJoinedService.GetCountByTallyJoinedAsync(_tally.TallyId);
        var decryptionShares = await _decryptionShareService.GetCountByTallyAsync(_tally.TallyId);

        return decryptionShares == joinedGuardians;
    }

    private async Task<bool> ShouldRespondChallenge()
    {
        return !await _challengeResponseService.GetExistsByTallyAsync(
            _tally.TallyId,
            _authenticationService.UserName ?? string.Empty);
    }

    private async Task<bool> ShouldVerifyChallenge()
    {
        return await Task.FromResult(_authenticationService.IsAdmin && _tally.State != TallyState.AdminVerifyChallenge);
    }
    #endregion

    #region Run Steps
    private async Task RespondChallenge()
    {
        await _tallyManager.ComputeChallengeResponse(
            _authenticationService.UserName!,
            _tally);
    }

    private async Task DecryptShares()
    {
        await _tallyManager.DecryptShare(
            _authenticationService.UserName!,
            _tally
            );
    }

    private async Task VerifyChallenge()
    {
        await _tallyService.UpdateStateAsync(_tally.TallyId, TallyState.AdminVerifyChallenge);

        await _tallyManager.ValidateChallengeResponse(_tally);

        await _tallyService.UpdateStateAsync(_tally.TallyId, TallyState.Complete);
    }

    private async Task GenerateChallenge()
    {
        await _tallyService.UpdateStateAsync(_tally.TallyId, TallyState.AdminGenerateChallenge);

        await _tallyManager.CreateChallenge(_tally);

        await _tallyService.UpdateStateAsync(_tally.TallyId, TallyState.PendingGuardianRespondChallenge);
    }

    private async Task AccumulateTally()
    {
        await _tallyService.UpdateStateAsync(_tally.TallyId, TallyState.AdminAccumulateTally);

        await _tallyManager.AccumulateAllUploadTallies(_tally);

        await _tallyService.UpdateStateAsync(_tally.TallyId, TallyState.PendingGuardianDecryptShares);
    }

    #endregion

    private void GenerateGuardianSteps()
    {
        var guardianSteps = new List<StateMachineStep<TallyState>>()
        {
            new()
            {
                State = TallyState.PendingGuardianDecryptShares,
                ShouldRunStep = ShouldDecryptShares,
                RunStep = DecryptShares,
            },
            new()
            {
                State = TallyState.PendingGuardianRespondChallenge,
                ShouldRunStep = ShouldRespondChallenge,
                RunStep = RespondChallenge,
            },
        };

        _steps.Add(false, guardianSteps);
    }
    private void GenerateAdminSteps()
    {
        var adminSteps = new List<StateMachineStep<TallyState>>()
        {
            new()
            {
                State = TallyState.PendingGuardiansJoin,
                ShouldRunStep = ShouldStartTally,
                RunStep = AccumulateTally,
            },
            new()
            {
                State = TallyState.PendingGuardianDecryptShares,
                ShouldRunStep = ShouldGenerateChallenge,
                RunStep = GenerateChallenge,
            },
            new()
            {
                State = TallyState.PendingGuardianRespondChallenge,
                ShouldRunStep = ShouldVerifyChallenge,
                RunStep = VerifyChallenge,
            },

            // these are error states, allowing the tally to continue if failure occurs
            new()
            {
                State = TallyState.AdminAccumulateTally,
                ShouldRunStep = AlwaysRun,
                RunStep = AccumulateTally,
            },
            new()
            {
                State = TallyState.AdminGenerateChallenge,
                ShouldRunStep = AlwaysRun,
                RunStep = GenerateChallenge,
            },
            new()
            {
                State = TallyState.AdminVerifyChallenge,
                ShouldRunStep = AlwaysRun,
                RunStep = VerifyChallenge,
            }
        };

        _steps.Add(true, adminSteps);
    }
}
