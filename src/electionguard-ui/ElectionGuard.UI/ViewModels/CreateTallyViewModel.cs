using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.Input;

namespace ElectionGuard.UI.ViewModels;

[QueryProperty(ElectionIdParam, nameof(ElectionId))]
public partial class CreateTallyViewModel : BaseViewModel
{
    public const string ElectionIdParam = "ElectionId";

    private TallyService _tallyService;
    private ElectionService _electionService;
    private ManifestService _manifestService;
    private KeyCeremonyService _keyCeremonyService;
    private BallotUploadService _ballotUploadService;
    private BallotService _ballotService;

    public CreateTallyViewModel(
        IServiceProvider serviceProvider,
        TallyService tallyService,
        ManifestService manifestService,
        ElectionService electionService,
        KeyCeremonyService keyCeremonyService,
        BallotUploadService ballotUploadService,
        BallotService ballotService) : base("CreateTally", serviceProvider)
    {
        _tallyService = tallyService;
        _manifestService = manifestService;
        _electionService = electionService;
        _keyCeremonyService = keyCeremonyService;
        _ballotUploadService = ballotUploadService;
        _ballotService = ballotService;
    }

    [ObservableProperty]
    private string _electionId = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateTallyCommand))]
    private string _tallyName = string.Empty;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _useAllBallots = true;

    partial void OnUseAllBallotsChanged(bool value)
    {

    }

    [ObservableProperty]
    private Election _currentElection = new();

    [ObservableProperty]
    private KeyCeremonyRecord? _currentKeyCeremony;

    [ObservableProperty]
    private Manifest _currentManifest;

    [ObservableProperty]
    private ObservableCollection<string> _deviceList = new();

    [ObservableProperty]
    private ObservableCollection<string> _dateList = new();

    [ObservableProperty]
    private ObservableCollection<BallotUpload> _ballotUploads = new();

    partial void OnElectionIdChanged(string value)
    {
        _ = Task.Run(async () =>
        {
            var currentElection = await _electionService.GetByElectionIdAsync(value);
            var record = await _manifestService.GetByElectionIdAsync(value);
            var uploads = await _ballotUploadService.GetByElectionIdAsync(value);
            var startDate = uploads.Min(u => u.BallotsStart);
            var endDate = uploads.Max(u => u.BallotsEnd);

            _ = Shell.Current.CurrentPage.Dispatcher.Dispatch(async () =>
            {
                CurrentElection = currentElection!;
                BallotUploads = uploads.DistinctBy(u => u.DeviceId).ToObservableCollection();
                CurrentManifest = new(record?.ManifestData);
                CurrentKeyCeremony = await _keyCeremonyService.GetByKeyCeremonyIdAsync(CurrentElection.KeyCeremonyId!);
                TallyName = $"{CurrentElection.Name} {AppResources.TallyText}";
                DateList.Clear();
                for (DateTime currentDate = startDate; currentDate <= endDate; currentDate = currentDate.AddDays(1))
                {
                    DateList.Add(currentDate.ToShortDateString());
                }
            });
        });
    }

    [RelayCommand(CanExecute = nameof(CanCreateTally))]
    private async Task CreateTally()
    {
        // calculate ballot count and upload count
        var ballotCount = await _ballotService.GetCountBallotsByElectionIdStateAsync(ElectionId, BallotBoxState.Cast);
        var challengedCount = await _ballotService.GetCountBallotsByElectionIdStateAsync(ElectionId, BallotBoxState.Challenged);
        var spoiledCount = await _ballotService.GetCountBallotsByElectionIdStateAsync(ElectionId, BallotBoxState.Spoiled);

        TallyRecord newTally = new()
        {
            Name = TallyName,
            ElectionId = ElectionId,
            KeyCeremonyId = CurrentElection.KeyCeremonyId,
            Quorum = CurrentKeyCeremony.Quorum,
            NumberOfGuardians = CurrentKeyCeremony.NumberOfGuardians,
            CastBallotCount = ballotCount,
            ChallengedBallotCount = challengedCount,
            SpoiledBallotCount = spoiledCount,
            State = TallyState.PendingGuardiansJoin,
        };

        _ = await _tallyService.SaveAsync(newTally);

        var pageParams = new Dictionary<string, object>
            {
                { TallyProcessViewModel.CurrentTallyIdParam, newTally.TallyId }
            };
        await NavigationService.GoToPage(typeof(TallyProcessViewModel), pageParams);

    }

    private bool CanCreateTally()
    {
        return !string.IsNullOrWhiteSpace(TallyName);
    }

    public override async Task OnAppearing()
    {
        await base.OnAppearing();
    }

}
