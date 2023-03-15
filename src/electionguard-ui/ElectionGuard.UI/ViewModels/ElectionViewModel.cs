using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;

namespace ElectionGuard.UI.ViewModels;

[QueryProperty(CurrentElectionParam, nameof(CurrentElection))]
public partial class ElectionViewModel : BaseViewModel
{
    private KeyCeremonyService _keyCeremonyService;
    private ManifestService _manifestService;
    private BallotUploadService _uploadService;

    public ElectionViewModel(IServiceProvider serviceProvider, KeyCeremonyService keyCeremonyService, ManifestService manifestService, BallotUploadService uploadService) : base(null, serviceProvider)
    {
        _keyCeremonyService = keyCeremonyService;
        _manifestService = manifestService;
        _uploadService = uploadService;
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
    private ObservableCollection<BallotUpload> _ballotUploads = new();

    [ObservableProperty]
    private ObservableCollection<Tally> _tallies = new();


    partial void OnCurrentElectionChanged(Election? value)
    {
        PageTitle = value?.Name ?? "";
        _ = Task.Run(async () =>
        {
            ManifestRecord = await _manifestService.GetByElectionIdAsync(value?.ElectionId);
            KeyCeremony = await _keyCeremonyService.GetByKeyCeremonyIdAsync(value?.KeyCeremonyId);
            var uploads = await _uploadService.GetByElectionIdAsync(value?.ElectionId);
            _ballotUploads.Clear();
            uploads.ForEach((upload) => {
                _ballotUploads.Add(upload);
            });
            
            _tallies.Clear();

        });
    }

    [RelayCommand]
    private async Task AddBallots()
    {
        var pageParams = new Dictionary<string, object>
            {
                { BallotUploadViewModel.ElectionIdParam, CurrentElection.ElectionId }
            };

        await NavigationService.GoToPage(typeof(BallotUploadViewModel), pageParams);
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
