using System.Threading;
using ElectionGuard.Decryption.Shares;
using ElectionGuard.ElectionSetup;
using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.Services;

public class TallyStateMachine : ITallyStateMachine
{
    private Dictionary<bool, List<StateMachineStep<TallyState, TallyRecord>>> _steps = new();

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

    public async Task<bool> Run(TallyRecord tally)
    {
        bool ran = false;
        string label = $"Electionguard.UI.TallyStateMachine-{_authenticationService.UserName}";

        using var mutex = new Mutex(true, label, out var owned);
        if (owned)
        {
            try
            {
                if (mutex.WaitOne(10))
                {
                    Task.WaitAll(RunAsync(tally));
                    ran = true;
                }
            }
            catch (AbandonedMutexException)
            {
                mutex.ReleaseMutex();
            }
            catch
            {
                throw;
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }
        return ran;
    }

    private async Task RunAsync(TallyRecord tally)
    {
        try
        {
            var steps = _steps[_authenticationService.IsAdmin];
            var currentStep = steps.SingleOrDefault(s => s.State == tally.State);
            if (currentStep is not null && await currentStep.ShouldRunStep(tally))
            {
                await currentStep.RunStep(tally);
            }
        }
        catch (Exception)
        {
            throw;
        }
    }



    #region Should Run
    private async Task<bool> ShouldAutoStartTally(TallyRecord tally)
    {
        const bool GUARDIAN_JOINED_TALLY = true;

        var joinedGuardians = await _tallyJoinedService.GetGuardianCountByTallyAsync(tally.TallyId);
        joinedGuardians.TryGetValue(GUARDIAN_JOINED_TALLY, out var consentCount);
        joinedGuardians.TryGetValue(!GUARDIAN_JOINED_TALLY, out var rejectCount);

        var isQuorumReached = consentCount >= tally.Quorum;
        var haveAllGuardiansJoined = (consentCount + rejectCount) == tally.NumberOfGuardians;

        return isQuorumReached && haveAllGuardiansJoined;
    }

    private async Task<bool> ShouldDecryptShares(TallyRecord tally)
    {
        return !await _decryptionShareService.GetExistsByTallyAsync(tally.TallyId, _authenticationService.UserName ?? string.Empty);
    }

    private async Task<bool> ShouldGenerateChallenge(TallyRecord tally)
    {
        var joinedGuardians = await _tallyJoinedService.GetCountByTallyJoinedAsync(tally.TallyId);
        var decryptionShares = await _decryptionShareService.GetCountByTallyAsync(tally.TallyId);

        return decryptionShares == joinedGuardians;
    }

    private async Task<bool> ShouldRespondChallenge(TallyRecord tally)
    {
        return !await _challengeResponseService.GetExistsByTallyAsync(
            tally.TallyId,
            _authenticationService.UserName ?? string.Empty);
    }

    private async Task<bool> ShouldVerifyChallenge(TallyRecord tally)
    {
        var joinedGuardians = await _tallyJoinedService.GetCountByTallyJoinedAsync(tally.TallyId);
        var challengeResponse = await _challengeResponseService.GetCountByTallyAsync(tally.TallyId);

        return challengeResponse == joinedGuardians;
    }
    #endregion

    #region Run Steps

    private async Task StartTally(TallyRecord tally)
    {
        await _tallyService.UpdateStateAsync(tally.TallyId, TallyState.TallyStarted);
    }

    private async Task RespondChallenge(TallyRecord tally)
    {
        await _tallyManager.ComputeChallengeResponse(
            _authenticationService.UserName!,
            tally);
    }

    private async Task DecryptShares(TallyRecord tally)
    {
        await _tallyManager.DecryptShare(
            _authenticationService.UserName!,
            tally
            );
    }

    private async Task VerifyChallenge(TallyRecord tally)
    {
        await _tallyService.UpdateStateAsync(tally.TallyId, TallyState.AdminVerifyChallenge);

        await _tallyManager.ValidateChallengeResponse(tally);

        await _tallyService.UpdateStateAsync(tally.TallyId, TallyState.Complete);
    }

    private async Task GenerateChallenge(TallyRecord tally)
    {
        await _tallyService.UpdateStateAsync(tally.TallyId, TallyState.AdminGenerateChallenge);

        await _tallyManager.CreateChallenge(tally);

        await _tallyService.UpdateStateAsync(tally.TallyId, TallyState.PendingGuardianRespondChallenge);
    }

    private async Task AccumulateTally(TallyRecord tally)
    {
        await _tallyService.UpdateStateAsync(tally.TallyId, TallyState.AdminAccumulateTally);

        await _tallyManager.AccumulateAllUploadTallies(tally);

        await _tallyService.UpdateStateAsync(tally.TallyId, TallyState.PendingGuardianDecryptShares);
    }

    #endregion

    private void GenerateGuardianSteps()
    {
        var guardianSteps = new List<StateMachineStep<TallyState, TallyRecord>>()
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
        var adminSteps = new List<StateMachineStep<TallyState, TallyRecord>>()
        {
            new()
            {
                State = TallyState.PendingGuardiansJoin,
                ShouldRunStep = ShouldAutoStartTally,
                RunStep = StartTally,
            },
            new()
            {
                State = TallyState.TallyStarted,
                ShouldRunStep = StateMachineStep<TallyState, TallyRecord>.AlwaysRun,
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
                ShouldRunStep = StateMachineStep<TallyState, TallyRecord>.AlwaysRun,
                RunStep = AccumulateTally,
            },
            new()
            {
                State = TallyState.AdminGenerateChallenge,
                ShouldRunStep = StateMachineStep < TallyState, TallyRecord >.AlwaysRun,
                RunStep = GenerateChallenge,
            },
            new()
            {
                State = TallyState.AdminVerifyChallenge,
                ShouldRunStep = StateMachineStep < TallyState, TallyRecord >.AlwaysRun,
                RunStep = VerifyChallenge,
            }
        };

        _steps.Add(true, adminSteps);
    }
}
