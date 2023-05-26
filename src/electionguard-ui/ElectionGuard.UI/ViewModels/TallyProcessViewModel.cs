using CommunityToolkit.Mvvm.Input;
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
    [NotifyCanExecuteChangedFor(nameof(JoinTallyCommand))]
    [NotifyCanExecuteChangedFor(nameof(RejectTallyCommand))]
    private TallyRecord? _tally = default;

    [ObservableProperty]
    private Election _currentElection = new();

    [ObservableProperty]
    private List<BallotUpload> _ballotUploads = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartTallyCommand))]
    private ObservableCollection<GuardianTallyItem> _joinedGuardians = new();

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

    partial void OnTallyChanged(TallyRecord? oldValue, TallyRecord? newValue)
    {
        if (newValue is not null && oldValue?.TallyId != newValue?.TallyId)
        {
            _ = Shell.Current.CurrentPage.Dispatcher.DispatchAsync(async () =>
            {
                var electionId = newValue.ElectionId ?? string.Empty;
                var election = await _electionService.GetByElectionIdAsync(electionId);
                if (election is null)
                {
                    throw new ElectionGuardException($"ElectionId {electionId} not found! Tally {newValue.Id}");
                    // TODO: put up some error message somewhere, over the rainbow.
                }

                CurrentElection = election;

                BallotUploads = await _ballotUploadService.GetByElectionIdAsync(electionId);
                
                await UpdateTallyData();
            });
        }
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

        if (!_timer.IsRunning)
        {
            _timer.Start();
        }
    }

    private bool CanJoinTally() => Tally?.State == TallyState.PendingGuardiansJoin && !CurrentUserJoinedAlready();
    private bool CurrentUserJoinedAlready() => JoinedGuardians.SingleOrDefault(g => g.Name == UserName) is object;

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
        if (!_timer.IsRunning)
        {
            _timer.Start();
        }
    }

    private bool CanStartTally()
    {
        // add count >= quorum
        return Tally?.State == TallyState.PendingGuardiansJoin && !CurrentUserJoinedAlready();
    }


    public override async Task OnAppearing()
    {
        await base.OnAppearing();
        _timer.Tick += CeremonyPollingTimer_Tick;

        _timer.Start();
    }
    private void CeremonyPollingTimer_Tick(object? sender, EventArgs e)
    {
        if(Tally is null)
        {
            return;
        }

        if (Tally.State == TallyState.Complete)
        {
            _timer.Stop();
            return;
        }

        _ = Task.Run(async () =>
        {
            Tally = await _tallyService.GetByTallyIdAsync(TallyId);
            await UpdateTallyData();
            await _tallyRunner.Run(Tally);
        });
    }

    private async Task UpdateTallyData()
    {
        // if we have fewer than max number, see if anyone else joined
        if (JoinedGuardians.Count != Tally?.NumberOfGuardians)
        {
            var localData = await _tallyJoinedService.GetAllByTallyIdAsync(TallyId);

            foreach (var item in localData)
            {
                if (!JoinedGuardians.Any(g => g.Name == item.GuardianId))
                {
                    JoinedGuardians.Add(new GuardianTallyItem() { Name = item.GuardianId });
                }
            }
        }
        foreach (var guardian in JoinedGuardians)
        {
            guardian.HasDecryptShares = await _decryptionShareService.GetExistsByTallyAsync(TallyId, guardian.Name);
            guardian.HasResponse = await _challengeResponseService.GetExistsByTallyAsync(TallyId, guardian.Name);
            guardian.IsSelf = guardian.Name == UserName!;
        }
    }

}
