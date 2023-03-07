using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.Input;
using MongoDB.Driver.Core.Events;

namespace ElectionGuard.UI.ViewModels;

public partial class CreateElectionViewModel : BaseViewModel
{
    private readonly KeyCeremonyService _keyCeremonyService;
    private readonly ElectionService _electionService;
    private readonly ManifestService _manifestService;
    private readonly ContextService _contextService;
    private readonly ConstantsService _constantsService;
    private const string PageName = "CreateElection";

    public CreateElectionViewModel(IServiceProvider serviceProvider, KeyCeremonyService keyCeremonyService, ElectionService electionService, ManifestService manifestService, ContextService contextService, ConstantsService constantsService) : base(PageName, serviceProvider)
    {
        _keyCeremonyService = keyCeremonyService;
        _electionService = electionService;
        _manifestService = manifestService;
        _contextService = contextService;
        _constantsService = constantsService;
    }

    public override async Task OnAppearing()
    {
        KeyCeremonies = await _keyCeremonyService.GetAllCompleteAsync();
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
    private KeyCeremony? _keyCeremony = null;

    [ObservableProperty]
    private List<KeyCeremony> _keyCeremonies;

    [ObservableProperty]
    private string _electionUrl;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateElectionCommand))]
    private string _manifestNames;

    private List<FileResult> _manifestFiles = new();

    [ObservableProperty]
    private bool _isNameEnabled = true;

    [RelayCommand(CanExecute = nameof(CanCreate))]
    private async Task CreateElection()
    {
        var multiple = _manifestFiles.Count > 1;
        var lastElectionId = string.Empty;

        for (var i = 0; i < _manifestFiles.Count; i++)
        {
            var file = _manifestFiles[i];
            // create an election for each file
            try
            {
                // create the manifest
                using var manifest = new Manifest(File.ReadAllText(file.FullPath));
                var electionName = multiple ? manifest.Name.GetTextAt(0).Value : ElectionName;

                // check if the election anme exists
                var election = new Election(KeyCeremony.KeyCeremonyId, await MakeNameUnique(electionName), ElectionUrl, UserName!);
                lastElectionId = election.ElectionId;

                // create the context
                using var context = new CiphertextElectionContext(
                                                            (ulong)KeyCeremony.NumberOfGuardians,
                                                            (ulong)KeyCeremony.Quorum,
                                                            KeyCeremony.JointKey.JointPublicKey,
                                                            KeyCeremony.JointKey.CommitmentHash,
                                                            manifest.CryptoHash());

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
            }
            catch (Exception)
            {
                ErrorMessage += $"{AppResources.ErrorCreatingElection} - {file.FileName}\n";
            }
        }

        if (ErrorMessage == null)
        {
            // goto the email page or go to the home page
            if (multiple)
            {
                HomeCommand.Execute(null);
            }
            else
            {
                var election = await _electionService.GetByElectionIdAsync(lastElectionId);
                var pageParams = new Dictionary<string, object>
                {
                    { ElectionViewModel.CurrentElectionParam, election }
                };
                await NavigationService.GoToPage(typeof(ElectionViewModel), pageParams);
            }
        }
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
        return ManifestErrorMessage == string.Empty && _manifestFiles.Any() && ElectionName.Any() && KeyCeremony != null;
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
        ManifestErrorMessage = string.Empty;
        IsNameEnabled = _manifestFiles.Count <= 1;
        if (IsNameEnabled)
        {
            try
            {
                using var manifest = new Manifest(File.ReadAllText(_manifestFiles.First().FullPath));
                ElectionName = manifest.Name.GetTextAt(0).Value;
            }
            catch (Exception)
            {
                ManifestErrorMessage = AppResources.ErrorLoadingManifest;
                ElectionName = string.Empty;
            }
        }
        else
        {
            _manifestFiles.ForEach(file =>
            {
                try
                {
                    using var manifest = new Manifest(File.ReadAllText(file.FullPath));
                    if (!manifest.IsValid())
                    {
                        badFiles.Add(file.FileName);
                    }
                }
                catch(Exception)
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
    }

}
