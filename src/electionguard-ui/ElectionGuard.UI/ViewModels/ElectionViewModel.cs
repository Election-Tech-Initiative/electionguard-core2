using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using ElectionGuard.UI.Models;

namespace ElectionGuard.UI.ViewModels;

[QueryProperty(CurrentElectionParam, nameof(CurrentElection))]
[QueryProperty(ElectionIdParam, nameof(ElectionId))]
public partial class ElectionViewModel : BaseViewModel
{
    public const string CurrentElectionParam = "CurrentElection";
    public const string ElectionIdParam = "ElectionId";

    private readonly IStorageService _storageService;
    private readonly IStorageService _driveService;
    private readonly KeyCeremonyService _keyCeremonyService;
    private readonly ManifestService _manifestService;
    private readonly BallotUploadService _uploadService;
    private readonly ElectionService _electionService;
    private readonly TallyService _tallyService;
    private readonly ConstantsService _constantsService;
    private readonly ContextService _contextService;

    public ElectionViewModel(
        IServiceProvider serviceProvider,
        KeyCeremonyService keyCeremonyService,
        ManifestService manifestService,
        ContextService contextService,
        ConstantsService constantsService,
        BallotUploadService uploadService,
        ElectionService electionService,
        TallyService tallyService,
        ZipStorageService zipStorageService,
        IStorageService driveService) : base(null, serviceProvider)
    {
        _keyCeremonyService = keyCeremonyService;
        _manifestService = manifestService;
        _uploadService = uploadService;
        _electionService = electionService;
        _tallyService = tallyService;
        _constantsService = constantsService;
        _contextService = contextService;
        _storageService = zipStorageService;
        _driveService = driveService;
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddBallotsCommand))]
    private Election? _currentElection;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ViewCommand))]
    private bool _isViewing;

    [ObservableProperty]
    private string _electionId;

    [ObservableProperty]
    private Manifest? _manifest;

    [ObservableProperty]
    private KeyCeremonyRecord? _keyCeremony;

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
    [NotifyCanExecuteChangedFor(nameof(ReviewChallengedCommand))]
    private long _ballotSpoiledTotal = 0;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateTallyCommand))]
    [NotifyCanExecuteChangedFor(nameof(ReviewChallengedCommand))]
    private long _ballotChallengedTotal = 0;

    [ObservableProperty]
    private long _ballotDuplicateTotal = 0;

    [ObservableProperty]
    private long _ballotRejectedTotal = 0;

    [ObservableProperty]
    private ObservableCollection<BallotUpload> _ballotUploads = new();

    [ObservableProperty]
    private ObservableCollection<TallyRecord> _tallies = new();

    [ObservableProperty]
    private DateTime _ballotsUploadedDateTime;

    [ObservableProperty]
    private bool _step1Complete;

    [ObservableProperty]
    private bool _step2Complete;

    [ObservableProperty]
    private bool _step3Complete;

    [ObservableProperty]
    private bool _step4Complete;

    [ObservableProperty]
    private bool _step5Complete;

    [ObservableProperty]
    private TallyRecord? _currentTally;

    partial void OnBallotsUploadedDateTimeChanged(DateTime value)
    {
        Step2Complete = true;
    }

    partial void OnCurrentTallyChanged(TallyRecord? value)
    {
        if (value is null)
        {
            return;
        }

        if (value.State == TallyState.Complete)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
                await NavigationService.GoToPage(typeof(ViewTallyViewModel), new Dictionary<string, object>
                {
                    { "TallyId", value.TallyId! }
                }));
        }
        else if (value.State != TallyState.Abandoned)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            await NavigationService.GoToPage(typeof(TallyProcessViewModel), new Dictionary<string, object>
            {
                { "TallyId", value.TallyId! }
            }));
        }
    }

    private async Task RefreshUploads(string electionId)
    {
        var uploads = await _uploadService.GetByElectionIdAsync(electionId);
        BallotUploads.Clear();
        BallotCountTotal = 0;
        BallotAddedTotal = 0;
        BallotSpoiledTotal = 0;
        BallotChallengedTotal = 0;
        BallotDuplicateTotal = 0;
        BallotRejectedTotal = 0;
        uploads.ForEach((upload) =>
        {
            BallotUploads.Add(upload);
            BallotCountTotal += upload.BallotCount;
            BallotAddedTotal += upload.BallotImported;
            BallotChallengedTotal += upload.BallotChallenged;
            BallotSpoiledTotal += upload.BallotSpoiled;
            BallotDuplicateTotal += upload.BallotDuplicated;
            BallotRejectedTotal += upload.BallotRejected;
        });
    }

    partial void OnCurrentElectionChanged(Election? value)
    {
        PageTitle = value?.Name ?? "";
        _ = Shell.Current.CurrentPage.Dispatcher.DispatchAsync(async () =>
        {
            try
            {
                ManifestRecord = await _manifestService.GetByElectionIdAsync(value?.ElectionId!);
                KeyCeremony = await _keyCeremonyService.GetByKeyCeremonyIdAsync(value?.KeyCeremonyId!);
                await RefreshUploads(value?.ElectionId!);
                
                Tallies.Clear();
                var tallies = await _tallyService.GetAllActiveByElectionIdAsync(value?.ElectionId!);
                foreach (var item in tallies)
                {
                    Tallies.Add(item);
                }

                Step1Complete = CurrentElection?.ExportEncryptionDateTime != null;
                Step2Complete = BallotAddedTotal + BallotSpoiledTotal > 0;
                Step4Complete = Tallies.Count > 0;
                Step5Complete = Tallies.Count(t => t.LastExport != null) > 0;
            }
            catch (Exception)
            {
            }
        });
    }

    partial void OnElectionIdChanged(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        _ = Shell.Current.CurrentPage.Dispatcher.DispatchAsync(async () => CurrentElection = await _electionService.GetByElectionIdAsync(value));
    }

    [RelayCommand(CanExecute = nameof(CanCreateTally))]
    private async Task CreateTally()
    {
        var pageParams = new Dictionary<string, object>
            {
                { CreateTallyViewModel.ElectionIdParam, CurrentElection.ElectionId }
            };

        await NavigationService.GoToPage(typeof(CreateTallyViewModel), pageParams);
    }

    private bool CanCreateTally()
    {
        return BallotCountTotal > 0 || BallotChallengedTotal > 0;
    }

    [RelayCommand(CanExecute = nameof(CanUpload))]
    private async Task AddBallots()
    {
        var pageParams = new Dictionary<string, object>
            {
                { BallotUploadViewModel.ElectionIdParam, CurrentElection.ElectionId }
            };

        await NavigationService.GoToPage(typeof(BallotUploadViewModel), pageParams);
    }

    private bool CanUpload()
    {
        return CurrentElection?.ExportEncryptionDateTime != null;
    }

    [RelayCommand(CanExecute = nameof(CanReview))]
    private async Task ReviewChallenged()
    {
        var vm = Ioc.Default.GetService(typeof(ChallengedPopupViewModel)) as ChallengedPopupViewModel;

        // resetting the election so that the loading will be called a second time
        vm!.ElectionId = string.Empty;
        vm!.ElectionId = CurrentElection!.ElectionId!;

        await NavigationService.GoToModal(typeof(ChallengedPopupViewModel));
        await RefreshUploads(CurrentElection!.ElectionId!);
    }

    private bool CanReview()
    {
        return BallotChallengedTotal > 0;
    }

    [RelayCommand]
    private async Task ExportEncryption()
    {
        const string egDriveLabel = "egdrive";

        var answer = await Shell.Current.CurrentPage.DisplayAlert(
            AppResources.ExportDriveWarningTitle,
            AppResources.ExportDriveWarning,
            AppResources.YesText,
            AppResources.NoText);

        if (!answer)
        {
            return;
        }

        // check for any usb drives named egDrive
        var egDrives = from drive in DriveInfo.GetDrives()
                       where drive != null
                       where drive.DriveType == DriveType.Removable
                       where drive.IsReady
                       where drive.VolumeLabel.ToLower() == egDriveLabel
                       select drive;

        var context = await _contextService.GetByElectionIdAsync(CurrentElection.ElectionId);
        var constants = await _constantsService.GetByElectionIdAsync(CurrentElection.ElectionId);
        var manifest = await _manifestService.GetByElectionIdAsync(CurrentElection.ElectionId);

        if (context == null || constants == null || manifest == null)
        {
            // there's a data problem. This should never happen.
            return;
        }

        var encryptionPackage = new EncryptionPackage(context, constants, manifest);

        foreach (var drive in egDrives)
        {
            const string artifactFolder = "artifacts";

            var destinationFolder = Path.Combine(drive.Name, artifactFolder);
            _ = Directory.CreateDirectory(destinationFolder);

            _driveService.UpdatePath(destinationFolder);
            _driveService.ToFiles(encryptionPackage);

        }

        if (egDrives.Count() >= 0)
        {
            await MarkCurrentElectionAsExported();
        }
    }

    private async Task MarkCurrentElectionAsExported()
    {
        CurrentElection.ExportEncryptionDateTime = DateTime.UtcNow;
        await _electionService.UpdateEncryptionExportDateAsync(CurrentElection.ElectionId, CurrentElection.ExportEncryptionDateTime.Value);
        AddBallotsCommand.NotifyCanExecuteChanged();
        Step1Complete = true;
    }

    [RelayCommand(CanExecute = nameof(CanView))]
    private async Task View()
    {
        IsViewing = true;
        var vm = (ManifestViewModel)Ioc.Default.GetService(typeof(ManifestViewModel));
        vm.Manifest = new Manifest(Manifest.ToJson());

        await NavigationService.GoToModal(typeof(ManifestViewModel));
        IsViewing = false;
    }

    private bool CanView()
    {
        return !IsViewing;
    }

    partial void OnManifestRecordChanged(ManifestRecord value)
    {
        Manifest = new Manifest(value.ManifestData);
        ManifestName = Manifest.Name.GetTextAt(0).Value;
    }

    public override void Dispose()
    {
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    public override async Task OnAppearing()
    {
        await base.OnAppearing();
    }

}
