using CommunityToolkit.Mvvm.Input;
using ElectionGuard.UI.Lib.Services;
using MongoDB.Driver.Core.Clusters;

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

    public CreateTallyViewModel(IServiceProvider serviceProvider, TallyService tallyService, ManifestService manifestService, ElectionService electionService, KeyCeremonyService keyCeremonyService, BallotUploadService ballotUploadService) : base("CreateTally", serviceProvider)
    {
        _tallyService = tallyService;
        _manifestService = manifestService;
        _electionService = electionService;
        _keyCeremonyService = keyCeremonyService;
        _ballotUploadService = ballotUploadService;
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
    private Election _currentElection;

    [ObservableProperty]
    private KeyCeremonyRecord _currentKeyCeremony;

    [ObservableProperty]
    private Manifest _currentManifest;

    [ObservableProperty]
    private List<string> _deviceList = new();

    [ObservableProperty]
    private List<string> _dateList = new();

    [ObservableProperty]
    private List<BallotUpload> _ballotUploads = new();

    partial void OnElectionIdChanged(string value)
    {
        _ = Task.Run(async () =>
        {
            CurrentElection = await _electionService.GetByElectionIdAsync(value);
            var record = await _manifestService.GetByElectionIdAsync(value);
            CurrentManifest = new(record?.ManifestData);
            CurrentKeyCeremony = await _keyCeremonyService.GetByKeyCeremonyIdAsync(CurrentElection.KeyCeremonyId);
            TallyName = $"{CurrentElection.Name} {AppResources.TallyText}";

            var uploads = await _ballotUploadService.GetByElectionIdAsync(value);
            BallotUploads = uploads.DistinctBy(u => u.DeviceId).ToList();
            var startDate = uploads.Min(u => u.BallotsStart);
            var endDate = uploads.Max(u => u.BallotsEnd);
            DateList.Clear();
            for (DateTime currentDate = startDate; currentDate <= endDate; currentDate = currentDate.AddDays(1))
            {
                DateList.Add(currentDate.ToShortDateString());
            }

        });
    }

    [RelayCommand(CanExecute = nameof(CanCreateTally))]
    private async Task CreateTally()
    {
        // calculate ballot count and upload count
        var ballotCount = 0;
        var uploadCount = 0;

        TallyRecord newTally = new()
        {
            Name = TallyName,
            ElectionId = ElectionId,
            KeyCeremonyId = CurrentElection.KeyCeremonyId,
            Quorum = CurrentKeyCeremony.Quorum,
            NumberOfGuardians = CurrentKeyCeremony.NumberOfGuardians,
            BallotCount = ballotCount,
            BallotUploadCount = uploadCount,
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
