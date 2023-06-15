using CommunityToolkit.Mvvm.Input;
using ElectionGuard.UI.Lib.Extensions;
using ElectionGuard.UI.Models;
using ElectionGuard.UI.Services;

namespace ElectionGuard.UI.ViewModels;

[QueryProperty(CurrentTallyIdParam, nameof(TallyId))]
public partial class TallyProcessViewModel : BaseViewModel
{
    public const string CurrentTallyIdParam = "TallyId";

    [ObservableProperty]
    private string _tallyId = string.Empty;

    [ObservableProperty]
    private bool _canUserJoinTally;

    [ObservableProperty]
    private bool _canUserStartTally;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(JoinTallyCommand))]
    [NotifyCanExecuteChangedFor(nameof(StartTallyCommand))]
    [NotifyCanExecuteChangedFor(nameof(RejectTallyCommand))]
    [NotifyCanExecuteChangedFor(nameof(AbandonTallyCommand))]
    private TallyRecord? _tally = default;

    [ObservableProperty]
    private Election _currentElection = new();

    [ObservableProperty]
    private List<BallotUpload> _ballotUploads = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartTallyCommand))]
    private ObservableCollection<GuardianTallyItem> _joinedGuardians = new();

    [ObservableProperty]
    private TallyCeremonyChecklist _checklist = new();

    private readonly ElectionService _electionService;
    private readonly TallyService _tallyService;
    private readonly BallotUploadService _ballotUploadService;
    private readonly TallyJoinedService _tallyJoinedService;
    private readonly ITallyStateMachine _tallyRunner;
    private readonly DecryptionShareService _decryptionShareService;
    private readonly ChallengeResponseService _challengeResponseService;

    public TallyProcessViewModel(
        IServiceProvider serviceProvider,
        TallyService tallyService,
        TallyJoinedService tallyJoinedService,
        ElectionService electionService,
        BallotUploadService ballotUploadService,
        DecryptionShareService decryptionShareService,
        ChallengeResponseService challengeResponseService,
        ITallyStateMachine tallyRunner) :
        base("TallyProcess", serviceProvider)
    {
        _tallyService = tallyService;
        _electionService = electionService;
        _tallyJoinedService = tallyJoinedService;
        _ballotUploadService = ballotUploadService;
        _tallyRunner = tallyRunner;
        _decryptionShareService = decryptionShareService;
        _challengeResponseService = challengeResponseService;
    }

    partial void OnJoinedGuardiansChanged(ObservableCollection<GuardianTallyItem> value)
    {
        CanUserJoinTally = CanJoinTally(); // needs to be aware of who joined the tally.
    }

    partial void OnTallyChanged(TallyRecord? oldValue, TallyRecord? newValue)
    {
        if (newValue is null || oldValue?.TallyId == newValue?.TallyId)
        {
            return;
        }

        JoinedGuardians.Clear();

        _ = Shell.Current.CurrentPage.Dispatcher.DispatchAsync(async () =>
        {
            if (newValue?.State == TallyState.Abandoned)
            {
                await Shell.Current.CurrentPage.DisplayAlert(AppResources.AbandonTallyTitle, AppResources.AbandonTallyText, AppResources.OkText);
                await NavigationService.GoHome();

                return;
            }

            await UpdateElection(newValue);
            await UpdateTallyData();

            // Needs to be last. Needs to be aware of tally state change.
            CanUserJoinTally = CanJoinTally();
        });
    }

    private async Task UpdateElection(TallyRecord? newValue)
    {
        if (newValue is null)
        {
            throw new ArgumentException($"{nameof(UpdateElection)}: Invalid {typeof(TallyRecord)}");
        }

        ArgumentException.ThrowIfNullOrEmpty(newValue.ElectionId, nameof(newValue.ElectionId));

        var election = await _electionService.GetByElectionIdAsync(newValue.ElectionId);

        ElectionGuardException.ThrowIfNull(election, $"ElectionId {newValue.ElectionId} not found! Tally {newValue.Id}"); // This should never happen.

        CurrentElection = election;

        BallotUploads = await _ballotUploadService.GetByElectionIdAsync(newValue.ElectionId);
    }

    partial void OnTallyIdChanged(string value)
    {
        _ = Shell.Current.CurrentPage.Dispatcher.DispatchAsync(async () =>
        {
            Tally = await _tallyService.GetByTallyIdAsync(value);
        });
    }

    [RelayCommand(CanExecute = nameof(CanJoinTally))]
    private async Task JoinTally()
    {
        var joiner = new TallyJoinedRecord()
        {
            TallyId = TallyId,
            GuardianId = UserName!, // can assume not null, since you need to be signed into 
            Joined = true,
        };

        await _tallyJoinedService.JoinTallyAsync(joiner);

        if (!(_timer?.IsRunning ?? true))
        {
            _timer.Start();
        }
    }

    private bool CanJoinTally()
    {
        return Tally?.State == TallyState.PendingGuardiansJoin &&
            !CurrentUserJoinedAlready() &&
            !AuthenticationService.IsAdmin;
    }

    private bool CurrentUserJoinedAlready()
    {
        return JoinedGuardians.SingleOrDefault(g => g.Name == UserName) is not null;
    }

    // guardian pulls big T tally from mongo
    // Tally + Tally.

    [RelayCommand(CanExecute = nameof(CanJoinTally))]
    private async Task RejectTally()
    {
        var joiner = new TallyJoinedRecord()
        {
            TallyId = TallyId,
            GuardianId = UserName!, // can assume not null, since you need to be signed in to get here
        };

        await _tallyJoinedService.JoinTallyAsync(joiner);

        HomeCommand.Execute(null);
    }

    [RelayCommand(CanExecute = nameof(CanStartTally))]
    private async Task StartTally()
    {
        if (Tally is null)
        {
            return;
        }

        await _tallyService.UpdateStateAsync(Tally.TallyId, TallyState.TallyStarted);
        Tally.State = TallyState.TallyStarted;
    }

    [RelayCommand(CanExecute = nameof(CanAbandon))]
    private async Task AbandonTally()
    {
        if (Tally == null)
        {
            // should never hit this. handles null case.
            await NavigationService.GoHome();
            return;
        }

        await _tallyService.UpdateStateAsync(TallyId, TallyState.Abandoned);

        await NavigationService.GoToPage(typeof(ElectionViewModel), new()
        {
            { ElectionViewModel.CurrentElectionParam, CurrentElection },
        });
    }

    [RelayCommand]
    private async Task ViewTally()
    {
        await NavigationService.GoToPage(typeof(ViewTallyViewModel), new() { { nameof(TallyId), TallyId } });
    }

    private bool CanAbandon()
    {
        return AuthenticationService.IsAdmin &&
            Tally?.State == TallyState.PendingGuardiansJoin;
    }

    private bool CanStartTally()
    {
        if (Tally == null || JoinedGuardians.Count == 0)
        {
            return false;
        }

        // add count >= quorum
        var quorumReached = JoinedGuardians.Count(g => g.Joined) >= Tally.Quorum;

        return Tally?.State == TallyState.PendingGuardiansJoin &&
            AuthenticationService.IsAdmin &&
            quorumReached;
    }


    public override async Task OnAppearing()
    {
        await base.OnAppearing();
        _timer!.Tick += CeremonyPollingTimer_Tick;

        _timer?.Start();
    }
    private void CeremonyPollingTimer_Tick(object? sender, EventArgs e)
    {
        if (Tally is null)
        {
            return;
        }

        if (Tally.State == TallyState.Complete)
        {
            _timer?.Stop();
            return;
        }

        _ = Task.Run(async () =>
        {
            Tally = await _tallyService.GetByTallyIdAsync(TallyId);
            if (Tally != null)
            {
                await UpdateTallyData();
                await _tallyRunner.Run(Tally);
            }
        });
    }

    private async Task UpdateTallyData()
    {
        if (Tally == null)
        {
            return;
        }

        // if we have fewer than max number, see if anyone else joined
        if (JoinedGuardians.Count != Tally?.NumberOfGuardians)
        {
            var newGuardians =
                from joinedGuardian in await _tallyJoinedService.GetAllByTallyIdAsync(TallyId)
                let tallyItem = new GuardianTallyItem
                {
                    Name = joinedGuardian.GuardianId,
                    Joined = joinedGuardian.Joined,
                }
                where !JoinedGuardians.Contains(tallyItem)
                select tallyItem;
            
            JoinedGuardians.AddRange(newGuardians);
        }
        foreach (var guardian in JoinedGuardians)
        {
            guardian.HasDecryptShares = await _decryptionShareService.GetExistsByTallyAsync(TallyId, guardian.Name);
            guardian.HasResponse = await _challengeResponseService.GetExistsByTallyAsync(TallyId, guardian.Name);
            guardian.IsSelf = guardian.Name == UserName!;
        }

        var sharesComputed = JoinedGuardians.Count(g => g.HasDecryptShares);
        var challengesResponded = JoinedGuardians.Count(g => g.HasResponse);

        Checklist = new TallyCeremonyChecklist(
            Tally!,
            sharesComputed,
            challengesResponded
            );
    }

}
