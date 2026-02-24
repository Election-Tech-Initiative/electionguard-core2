using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using ElectionGuard.UI.Models;

namespace ElectionGuard.UI.ViewModels;

public partial class CreateElectionViewModel : BaseViewModel
{
    private readonly KeyCeremonyService _keyCeremonyService;
    private readonly ElectionService _electionService;
    private readonly ManifestService _manifestService;
    private readonly ContextService _contextService;
    private readonly ConstantsService _constantsService;
    private const string PageName = "CreateElection";

    public CreateElectionViewModel(IServiceProvider serviceProvider,
                                   KeyCeremonyService keyCeremonyService,
                                   ElectionService electionService,
                                   ManifestService manifestService,
                                   ContextService contextService,
                                   ConstantsService constantsService,
                                   ILogger<CreateElectionViewModel> logger) : base(PageName, serviceProvider)
    {
        _keyCeremonyService = keyCeremonyService;
        _electionService = electionService;
        _manifestService = manifestService;
        _contextService = contextService;
        _constantsService = constantsService;
        _logger = logger;
    }

    public override async Task OnAppearing()
    {
        await base.OnAppearing();

        try
        {
            KeyCeremonies = await _keyCeremonyService.GetAllCompleteAsync() ?? new();
        }
        catch (Exception e)
        {
            _logger.LogError($"{nameof(_keyCeremonyService.GetAllCompleteAsync)} error: {e}");
            await Logout();
        }
    }

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateElectionCommand))]
    private string? _manifestErrorMessage;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateElectionCommand))]
    private string _electionName = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateElectionCommand))]
    private KeyCeremonyRecord? _keyCeremony = null;

    [ObservableProperty]
    private List<KeyCeremonyRecord> _keyCeremonies = new();

    [ObservableProperty]
    private string _electionUrl = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateElectionCommand))]
    private string _manifestNames = string.Empty;

    private List<FileResult> _manifestFiles = new();

    [ObservableProperty]
    private bool _isNameEnabled = true;

    [RelayCommand(CanExecute = nameof(CanCreate))]
    private async Task CreateElection()
    {
        ElectionGuardException.ThrowIfNull(KeyCeremony);
        ElectionGuardException.ThrowIfNull(KeyCeremony.JointKey);

        var multiple = _manifestFiles.Count > 1;
        ErrorMessage = string.Empty;

        await Parallel.ForEachAsync(_manifestFiles, async (file, cancel) =>
        {
            // create an election for each file
            try
            {
                ZipStorageService storageService = new();

                // create the manifest
                using var manifest = new Manifest(File.ReadAllText(file.FullPath));
                var electionName = multiple ? manifest.Name.GetTextAt(0).Value : ElectionName;

                // check if the election name exists
                var election = new Election()
                {
                    KeyCeremonyId = KeyCeremony.KeyCeremonyId,
                    Name = await MakeNameUnique(electionName),
                    ElectionUrl = ElectionUrl,
                    CreatedBy = UserName!
                };

                var extendedData = new LinkedList();
                if (!string.IsNullOrWhiteSpace(ElectionUrl))
                {
                    extendedData.Append("verification_url", ElectionUrl);
                }

                // create the context
                using var context = extendedData.Count == 0 ?
                    new CiphertextElectionContext(
                        (ulong)KeyCeremony.NumberOfGuardians,
                        (ulong)KeyCeremony.Quorum,
                        KeyCeremony.JointKey.JointPublicKey,
                        KeyCeremony.JointKey.CommitmentHash,
                        manifest.CryptoHash()) :
                    new CiphertextElectionContext(
                        (ulong)KeyCeremony.NumberOfGuardians,
                        (ulong)KeyCeremony.Quorum,
                        KeyCeremony.JointKey.JointPublicKey,
                        KeyCeremony.JointKey.CommitmentHash,
                        manifest.CryptoHash(), extendedData);

                var contextRecord = new ContextRecord() { ElectionId = election.ElectionId, ContextData = context.ToJson() };

                var constantsRecord = new ConstantsRecord() { ElectionId = election.ElectionId, ConstantsData = Constants.ToJson() };

                var manifestRecord = new ManifestRecord() { ElectionId = election.ElectionId, ManifestData = manifest.ToJson() };

                // save the election
                _ = await _electionService.SaveAsync(election);

                // save the context
                _ = await _contextService.SaveAsync(contextRecord);

                // save the constants
                _ = await _constantsService.SaveAsync(constantsRecord);

                // save the manifest
                _ = await _manifestService.SaveAsync(manifestRecord);

                if (multiple)
                {
                    // create the export file for each election
                    // need to add the path from the manifest
                    var zipPath = file.FullPath.ToLower().Replace(".json", ".zip");
                    var encryptionPackage = new EncryptionPackage(contextRecord, constantsRecord, manifestRecord);
                    storageService.UpdatePath(zipPath);
                    storageService.ToFiles(encryptionPackage);
                }

                if (!multiple)
                {
                    var loadElection = await _electionService.GetByElectionIdAsync(election.ElectionId);
                    var pageParams = new Dictionary<string, object>
                    {
                        { ElectionViewModel.CurrentElectionParam, loadElection }
                    };
                    await Shell.Current.CurrentPage.Dispatcher.DispatchAsync(async () =>
                    {
                        await NavigationService.GoToPage(typeof(ElectionViewModel), pageParams);
                    });
                }
            }
            catch (Exception)
            {
                ExceptionHandler.GetData(out var function, out var message, out var code);
                ErrorMessage += $"{AppResources.ErrorCreatingElection} - {file.FileName}\n";
            }
        }).ContinueWith((t) =>
        {
            if (string.IsNullOrEmpty(ErrorMessage))
            {
                // goto the email page or go to the home page
                if (multiple)
                {
                    _ = Shell.Current.CurrentPage.Dispatcher.DispatchAsync(() =>
                    {
                        HomeCommand.Execute(null);
                    });
                }
            }
        });
    }

    private async Task<string> MakeNameUnique(string electionName)
    {
        var count = 2;
        var name = electionName;
        bool found;
        do
        {
            found = await _electionService.ElectionNameExists(name);
            if (found)
            {
                name = $"{electionName} ({count++})";
            }
        } while (found);
        return name;
    }

    private bool CanCreate()
    {
        return string.IsNullOrEmpty(ManifestErrorMessage) && _manifestFiles.Any() && ElectionName.Any() && KeyCeremony != null;
    }

    [RelayCommand]
    private async Task PickManifestFiles()
    {
        var badFiles = new List<string>();
        var customFileType = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI, new[] { ".json" } }, // file extension
                    { DevicePlatform.macOS, new[] { "json" } }, // UTType values
                });
        var options = new PickOptions() { FileTypes = customFileType, PickerTitle = AppResources.SelectManifest };
        var files = await FilePicker.PickMultipleAsync(options);
        if (files == null || files.Count() == 0)
        {
            // if the picking was canceled then do not change anything
            return;
        }
        _manifestFiles.Clear();
        _manifestFiles.AddRange(files);
        ManifestErrorMessage = null;
        IsNameEnabled = _manifestFiles.Count <= 1;
        if (IsNameEnabled)
        {
            try
            {
                var data = File.ReadAllText(_manifestFiles.First().FullPath, System.Text.Encoding.UTF8);
                using var manifest = new Manifest(data);

                ElectionName = manifest.Name.GetTextAt(0).Value;
            }
            catch (Exception)
            {
                ExceptionHandler.GetData(out var function, out var message, out var code);
                ManifestErrorMessage = AppResources.ErrorLoadingManifest;
                ElectionName = string.Empty;
            }
        }
        else
        {
            _ = Parallel.ForEach(_manifestFiles, (file) =>
            {
                try
                {
                    using var manifest = new Manifest(File.ReadAllText(file.FullPath));
                    if (!manifest.IsValid())
                    {
                        badFiles.Add(file.FileName);
                    }
                }
                catch (Exception)
                {
                    badFiles.Add(file.FileName);
                }
            });
            ElectionName = AppResources.NameFromManifest;
        }

        // remove the last ", "
        var names = string.Empty;
        _manifestFiles.ForEach(file =>
        {
            if (!badFiles.Contains(file.FileName))
            {
                names += $"{file.FileName}, ";
            }
        });
        char[] trim = { ' ', ',' };
        ManifestNames = names.TrimEnd(trim);

        // remove the bad files from the manifest list of files
        badFiles.ForEach(file => _manifestFiles.Remove(_manifestFiles.First(f => f.FileName == file)));
        if (badFiles.Any())
        {
            var message = string.Empty;
            badFiles.ForEach(file => message += $"{file}, ");
            message = message.TrimEnd(trim);
            ManifestErrorMessage = $"{AppResources.ErrorManifest}: {message}\n{AppResources.RemovingList}";
        }

        // commenting out the display of the manifest until we can get it not throwing an exception
        //if (_manifestFiles.Count == 1)
        //{
        //    var vm = (ManifestViewModel)Ioc.Default.GetService(typeof(ManifestViewModel));
        //    vm.ManifestFile = _manifestFiles.First().FullPath;

        //    await NavigationService.GoToModal(typeof(ManifestViewModel));
        //}
    }

}
