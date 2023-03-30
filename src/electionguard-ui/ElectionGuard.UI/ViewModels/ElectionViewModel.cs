using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;

namespace ElectionGuard.UI.ViewModels;

[QueryProperty(CurrentElectionParam, nameof(CurrentElection))]
public partial class ElectionViewModel : BaseViewModel
{
    private KeyCeremonyService _keyCeremonyService;
    private ManifestService _manifestService;
    private BallotUploadService _uploadService;
    private ElectionService _electionService;

    public ElectionViewModel(IServiceProvider serviceProvider, KeyCeremonyService keyCeremonyService, ManifestService manifestService, BallotUploadService uploadService, ElectionService electionService) : base(null, serviceProvider)
    {
        _keyCeremonyService = keyCeremonyService;
        _manifestService = manifestService;
        _uploadService = uploadService;
        _electionService = electionService;
    }

    [ObservableProperty]
    private Election? _currentElection;

    [ObservableProperty]
    private Manifest? _manifest;

    [ObservableProperty]
    private KeyCeremony? _keyCeremony;

    [ObservableProperty]
    private ManifestRecord? _manifestRecord;

    [ObservableProperty]
    private string? _manifestName;

    [ObservableProperty]
    private long _ballotCountTotal = 0;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateTallyCommand))]
    private long _ballotAddedTotal = 0;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateTallyCommand))]
    private long _ballotSpoiledTotal = 0;

    [ObservableProperty]
    private long _ballotRejectedTotal = 0;

    [ObservableProperty]
    private ObservableCollection<BallotUpload> _ballotUploads = new();

    [ObservableProperty]
    private ObservableCollection<Tally> _tallies = new();

    [ObservableProperty]
    private DateTime _ballotsUploadedDateTime;

    partial void OnCurrentElectionChanged(Election? value)
    {
        PageTitle = value?.Name ?? "";
        _ = Task.Run(async () =>
        {
            ManifestRecord = await _manifestService.GetByElectionIdAsync(value?.ElectionId);
            KeyCeremony = await _keyCeremonyService.GetByKeyCeremonyIdAsync(value?.KeyCeremonyId);
            var uploads = await _uploadService.GetByElectionIdAsync(value?.ElectionId);
            BallotUploads.Clear();
            BallotCountTotal = 0;
            BallotAddedTotal = 0;
            BallotSpoiledTotal = 0;
            BallotRejectedTotal = 0;
            uploads.ForEach((upload) =>
            {
                BallotUploads.Add(upload);
                BallotCountTotal += upload.BallotCount;
                BallotAddedTotal += upload.BallotImported;
                BallotSpoiledTotal += upload.BallotSpoiled;
                BallotRejectedTotal += upload.BallotRejected;
            });

            Tallies.Clear();

        });
    }

    [RelayCommand(CanExecute = nameof(CanCreateTally))]
    private async Task CreateTally()
    {
        var pageParams = new Dictionary<string, object>
            {
                { BallotUploadViewModel.ElectionIdParam, CurrentElection.ElectionId }
            };

        // await NavigationService.GoToPage(typeof(BallotUploadViewModel), pageParams);
    }

    private bool CanCreateTally() => BallotCountTotal > 0 || BallotSpoiledTotal > 0;

    [RelayCommand(CanExecute = nameof(CanUpload))]
    private async Task AddBallots()
    {
        var pageParams = new Dictionary<string, object>
            {
                { BallotUploadViewModel.ElectionIdParam, CurrentElection.ElectionId }
            };

        await NavigationService.GoToPage(typeof(BallotUploadViewModel), pageParams);
    }

    private bool CanUpload() => CurrentElection?.ExportEncryptionDateTime != null;

    [RelayCommand]
    private async Task ExportEncryption()
    {
        var pageParams = new Dictionary<string, object>
            {
                { BallotUploadViewModel.ElectionIdParam, CurrentElection.ElectionId }
            };

        // await NavigationService.GoToPage(typeof(BallotUploadViewModel), pageParams);
        await _electionService.UpdateExportDateAsync(CurrentElection.Id, DateTime.Now);

        // this is for testing only and will be removed
        CurrentElection.ExportEncryptionDateTime = DateTime.Now;
        AddBallotsCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private async Task View()
    {
        var vm = (ManifestViewModel)Ioc.Default.GetService(typeof(ManifestViewModel));
        vm.Manifest = new Manifest(Manifest.ToJson());

        await NavigationService.GoToModal(typeof(ManifestViewModel));
    }

    partial void OnManifestRecordChanged(ManifestRecord value)
    {
        Manifest = new Manifest(value.ManifestData);
        ManifestName = Manifest.Name.GetTextAt(0).Value;
    }

    public const string CurrentElectionParam = "CurrentElection";

    public override void Dispose()
    {
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
