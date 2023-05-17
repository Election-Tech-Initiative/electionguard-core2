using CommunityToolkit.Mvvm.Input;

namespace ElectionGuard.UI.ViewModels;

[QueryProperty(CurrentTallyIdParam, nameof(TallyId))]
public partial class TallyProcessViewModel : BaseViewModel
{
    public const string CurrentTallyIdParam = "TallyId";

    [ObservableProperty]
    private string _tallyId = string.Empty;

    [ObservableProperty]
    private TallyRecord? _tally = default;

    [ObservableProperty]
    private Election _currentElection = new();

    [ObservableProperty]
    private List<BallotUpload> _ballotUploads = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(JoinTallyCommand))]
    private List<TallyJoinedRecord> _joinedGuardians = new();

    private ElectionService _electionService;
    private TallyService _tallyService;
    private BallotUploadService _ballotUploadService;
    private TallyJoinedService _tallyJoinedService;

    public TallyProcessViewModel(
        IServiceProvider serviceProvider,
        TallyService tallyService,
        TallyJoinedService tallyJoinedService,
        ElectionService electionService,
        BallotUploadService ballotUploadService) :
        base("TallyProcess", serviceProvider)
    {
        _tallyService = tallyService;
        _electionService = electionService;
        _tallyJoinedService = tallyJoinedService;
        _ballotUploadService = ballotUploadService;
    }

    partial void OnTallyChanged(TallyRecord? value)
    {
        if (value is not null)
        {
            _ = Shell.Current.CurrentPage.Dispatcher.DispatchAsync(async () =>
            {
                JoinedGuardians = await _tallyJoinedService.GetAllByTallyIdAsync(value.TallyId);
                var electionId = value.ElectionId ?? string.Empty;
                var election = await _electionService.GetByElectionIdAsync(electionId);
                if (election is null)
                {
                    throw new ElectionGuardException($"ElectionId {electionId} not found! Tally {value.Id}");
                    // TODO: put up some error message somewhere, over the rainbow.
                }

                CurrentElection = election;

                BallotUploads = await _ballotUploadService.GetByElectionIdAsync(electionId);

                JoinedGuardians = await _tallyJoinedService.GetAllByTallyIdAsync(value.TallyId);
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
        JoinedGuardians.Add(joiner);
    }

    private bool CanJoinTally() => Tally?.State == TallyState.PendingGuardiansJoin && !CurrentUserJoinedAlready();

    private bool CurrentUserJoinedAlready() => JoinedGuardians.SingleOrDefault(g => g.GuardianId == UserName) is object;

    [RelayCommand(CanExecute = nameof(CanJoinTally))]
    private async Task RejectTally()
    {
        var joiner = new TallyJoinedRecord()
        {
            TallyId = TallyId,
            GuardianId = UserName!, // can assume not null, since you need to be signed in to get here
        };

        await _tallyJoinedService.JoinTallyAsync(joiner);
        JoinedGuardians.Add(joiner);

        HomeCommand.Execute(null);
    }

    public override async Task OnAppearing()
    {
        await base.OnAppearing();
    }

}
