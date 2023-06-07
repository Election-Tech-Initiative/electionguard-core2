using System.Text;
using System.Text.Json;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.Input;
using ElectionGuard.Decryption;
using ElectionGuard.Decryption.Tally;

namespace ElectionGuard.UI.ViewModels;

[QueryProperty(ElectionIdParam, nameof(ElectionId))]
public partial class BallotUploadViewModel : BaseViewModel
{
    public const string ElectionIdParam = "ElectionId";

    [ObservableProperty]
    private string _electionId = string.Empty;

    [ObservableProperty]
    private string _fileErrorMessage = string.Empty;

    [ObservableProperty]
    private string _folderErrorMessage = string.Empty;

    [ObservableProperty]
    private BallotUploadPanel _showPanel = BallotUploadPanel.AutoUpload;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UploadCommand))]
    private string _deviceFile = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UploadCommand))]
    private string _ballotFolder = string.Empty;

    [ObservableProperty]
    private string _ballotFolderName = string.Empty;

    [ObservableProperty]
    private string _resultsText = string.Empty;

    [ObservableProperty]
    private string _uploadText = string.Empty;

    private uint _serialNumber;

    private ElementModQ? _manifestHash;

    private InternalManifest? _internalManifest;

    private CiphertextElectionContext? _context;

    partial void OnElectionIdChanged(string value)
    {
        _ = Task.Run(async () =>
        {
            var record = await _manifestService.GetByElectionIdAsync(value);
            using var manifest = new Manifest(record?.ManifestData);
            _manifestHash = new(manifest.CryptoHash());
            _internalManifest = new InternalManifest(manifest);

            var contextRecord = await _contextService.GetByElectionIdAsync(value);
            _context = new CiphertextElectionContext(contextRecord?.ContextData);
        });
    }

    [RelayCommand]
    private void Manual()
    {
        ShowPanel = BallotUploadPanel.ManualUpload;
        _timer?.Stop();
    }

    [RelayCommand]
    private void Auto()
    {
        ShowPanel = BallotUploadPanel.AutoUpload;
        _timer?.Start();
    }

    [RelayCommand]
    private async Task Cancel()
    {
        await NavigationService.GoToPage(typeof(ElectionViewModel), new() { { ElectionViewModel.ElectionIdParam, ElectionId } });
    }

    private async Task<string> ReadFileAsync(string path, CancellationToken cancellationToken = new())
    {
        if (!File.Exists(path))
        {
            return string.Empty;
        }

        using var stream = new StreamReader(path, Encoding.UTF8);
        return await stream.ReadToEndAsync(cancellationToken);
    }

    [RelayCommand(CanExecute = nameof(CanUpload))]
    private async Task Upload()
    {
        // create the device file
        var deviceData = await ReadFileAsync(DeviceFile);

        // TODO: enhance the EncryptionDevice object to have this info coming out of the json constructor
        EncryptionDevice device = new(deviceData);
        var deviceDocument = JsonDocument.Parse(deviceData);
        var location = string.Empty;
        long deviceId = -1;
        long sessionId = -1;
        long launchCode = -1;

        using (var jsonDoc = JsonDocument.Parse(deviceData))
        {
            location = jsonDoc.RootElement.GetProperty("location").GetString();
            deviceId = jsonDoc.RootElement.GetProperty("device_id").GetInt64();
            sessionId = jsonDoc.RootElement.GetProperty("session_id").GetInt64();
            launchCode = jsonDoc.RootElement.GetProperty("launch_code").GetInt64();
        }

        // save the ballot upload
        var ballots = Directory.GetFiles(BallotFolder);
        BallotUpload upload = new()
        {
            ElectionId = ElectionId,
            DeviceFileName = DeviceFile,
            DeviceFileContents = deviceData,
            Location = location,
            DeviceId = deviceId,
            SessionId = sessionId,
            LaunchCode = launchCode,
            BallotCount = ballots.LongLength,
            SerialNumber = _serialNumber,
            CreatedBy = UserName,
        };

        long totalCount = 0;
        long totalImported = 0;
        long totalDuplicated = 0;
        long totalRejected = 0;
        long totalChallenged = 0;
        long totalSpoiled = 0;
        var startDate = ulong.MaxValue;
        var endDate = ulong.MinValue;
        object tallyLock = new();

        var mediator = new TallyMediator();
        var ciphertextTally = mediator.CreateTally(Guid.NewGuid().ToString(),
            "subtally",
            _context!,
            _internalManifest!);

        UploadText = $"{AppResources.Uploading} {ballots.Length} {AppResources.Success2Text}";


        await Parallel.ForEachAsync(ballots, async (currentBallot, cancellationToken) =>
        {
            try
            {
                var filename = Path.GetFileName(currentBallot);
                var ballotData = await ReadFileAsync(currentBallot, cancellationToken);
                using var ballot = new CiphertextBallot(ballotData);

                var exists = await _ballotService.BallotExistsAsync(ballot.BallotCode.ToHex());

                if (ballot.ManifestHash != _manifestHash)
                {
                    _ = Interlocked.Increment(ref totalRejected);
                    return;
                }

                if (ballot.Timestamp < startDate)
                {
                    _ = Interlocked.Exchange(ref startDate, ballot.Timestamp);
                }
                if (ballot.Timestamp > endDate)
                {
                    _ = Interlocked.Exchange(ref endDate, ballot.Timestamp);
                }

                if (exists)
                {
                    _ = Interlocked.Increment(ref totalDuplicated);
                }
                else
                {
                    BallotRecord ballotRecord = new()
                    {
                        ElectionId = ElectionId,
                        TimeStamp = DateTime.UnixEpoch.AddSeconds(ballot.Timestamp),
                        UploadId = upload.UploadId,
                        FileName = filename,
                        BallotCode = ballot.BallotCode.ToHex(),
                        BallotState = ballot.State,
                        BallotData = ballotData,
                    };

                    _ = await _ballotService.SaveAsync(ballotRecord);

                    _ = ballot.State switch
                    {
                        BallotBoxState.Cast => Interlocked.Increment(ref totalImported),
                        BallotBoxState.Challenged => Interlocked.Increment(ref totalChallenged),
                        BallotBoxState.Spoiled => Interlocked.Increment(ref totalSpoiled),
                        BallotBoxState.NotSet => throw new NotImplementedException(),
                        BallotBoxState.Unknown => throw new NotImplementedException(),
                        _ => throw new NotImplementedException()
                    };

                    lock (tallyLock)
                    {
                        var result = ciphertextTally.Accumulate(ballot, true);
                    }
                }

                _ = Interlocked.Increment(ref totalCount);
                UploadText = $"{AppResources.SuccessText} {totalCount} / {ballots.Length} {AppResources.Success2Text}";
            }
            catch (Exception)
            {
            }
        });

        // update totals before saving
        upload.BallotCount = totalCount;
        upload.BallotImported = totalImported;
        upload.BallotDuplicated = totalDuplicated;
        upload.BallotRejected = totalRejected;
        upload.BallotSpoiled = totalSpoiled;
        upload.BallotChallenged = totalChallenged;

        upload.BallotsStart = DateTime.UnixEpoch.AddSeconds(startDate);
        upload.BallotsEnd = DateTime.UnixEpoch.AddSeconds(endDate);

        try
        {
            _ = await _uploadService.SaveAsync(upload);
            _timer?.Stop();
            ResultsText = $"{AppResources.SuccessText} {totalCount} {AppResources.Success2Text}";
            ShowPanel = BallotUploadPanel.Results;

            var record = new CiphertextTallyRecord()
            {
                ElectionId = ElectionId,
                UploadId = upload.UploadId!,
                IsExportable = false,
                CiphertextTallyData = ciphertextTally.ToJson()
            };
            _ = await _ciphertextTallyService.SaveAsync(record);
        }
        catch (Exception)
        {
        }
    }

    private bool CanUpload()
    {
        return DeviceFile != string.Empty && BallotFolder != string.Empty;
    }

    [RelayCommand]
    private async Task PickDeviceFile()
    {
        FileErrorMessage = string.Empty;
        var customFileType = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI, new[] { ".json" } }, // file extension
                    { DevicePlatform.macOS, new[] { "json" } }, // UTType values
                });
        var options = new PickOptions() { FileTypes = customFileType, PickerTitle = AppResources.SelectManifest };

        var file = await FilePicker.PickAsync(options);
        if (file == null)
        {
            FileErrorMessage = "No file was picked";
            // if the picking was canceled then do not change anything
            return;
        }
        // check if it is a device file. If it's not, this will throw an exception.
        try
        {
            var data = await ReadFileAsync(file.FullPath);
            EncryptionDevice device = new(data);
            DeviceFile = file.FullPath;
        }
        catch (Exception)
        {
            FileErrorMessage = "File is not a device file";
        }
    }

    [RelayCommand]
    private async Task PickBallotFolder()
    {
        CancellationToken token = new();
        try
        {
            var folder = await FolderPicker.Default.PickAsync(token);
            BallotFolder = folder.Folder!.Path;
            BallotFolderName = folder.Folder.Name;
            FolderErrorMessage = folder.Exception?.Message ?? string.Empty;
            // verify folder
        }
        catch (Exception ex)
        {
            BallotFolder = string.Empty;
            BallotFolderName = string.Empty;
            FolderErrorMessage = ex.Message;
        }
    }

    [RelayCommand]
    private void UploadMore()
    {
        ShowPanel = BallotUploadPanel.AutoUpload;
        ResultsText = string.Empty;
        UploadText = string.Empty;
        _lastDrive = -1;
        _timer?.Start();
    }

    private readonly BallotUploadService _uploadService;
    private readonly BallotService _ballotService;
    private readonly ManifestService _manifestService;
    private readonly ContextService _contextService;
    private readonly CiphertextTallyService _ciphertextTallyService;
    private bool _importing;
    private long _lastDrive = -1;

    public BallotUploadViewModel(IServiceProvider serviceProvider,
        BallotUploadService uploadService,
        BallotService ballotService,
        ManifestService manifestService,
        ContextService contextService,
        CiphertextTallyService ciphertextTallyService) : base("BallotUploadText", serviceProvider)
    {
        _uploadService = uploadService;
        _ballotService = ballotService;
        _manifestService = manifestService;
        _contextService = contextService;
        _ciphertextTallyService = ciphertextTallyService;

        _timer!.Tick += _timer_Tick;
        _timer?.Start();
    }

    private void _timer_Tick(object? sender, EventArgs e)
    {
        if (_importing)
        {
            return;
        }
        _importing = true;

        _ = Task.Run(async () =>
        {
            // check for a usb drive
            var drives = DriveInfo.GetDrives();
            foreach (var drive in drives)
            {
                if (drive.DriveType == DriveType.Removable && drive.VolumeLabel.ToLower() == "egdrive")
                {
                    _serialNumber = 0;
                    _ = StorageUtils.GetVolumeInformation(drive.Name, out _serialNumber);

                    if (_serialNumber == _lastDrive)
                    {
                        _importing = false;
                        return;
                    }

                    var used = await _uploadService.DriveUsed(_serialNumber, ElectionId);
                    if (used)
                    {
                        await Shell.Current.CurrentPage.Dispatcher.DispatchAsync(async () =>
                        {
                            var answer = await Shell.Current.CurrentPage.DisplayAlert(AppResources.DriveUsedText, AppResources.ImportAgainText, AppResources.YesText, AppResources.NoText);
                            if (!answer)
                            {
                                _lastDrive = _serialNumber;
                                _importing = false;
                                return;
                            }
                        });
                    }

                    var devicePath = Path.Combine(drive.Name, "artifacts", "encryption_devices");
                    if (!Directory.Exists(devicePath))
                    {
                        _importing = false;
                        return;
                    }

                    // find device file
                    var devices = Directory.GetFiles(devicePath);
                    foreach (var device in devices)
                    {
                        try
                        {
                            var data = await ReadFileAsync(device);
                            EncryptionDevice dev = new(data);
                            DeviceFile = device;
                            break;
                        }
                        catch (Exception)
                        {
                        }
                    }

                    // find submitted ballots folder
                    var ballotPath = Path.Combine(drive.Name, "artifacts", "encrypted_ballots");
                    if (!Directory.Exists(ballotPath))
                    {
                        DeviceFile = string.Empty;
                        _importing = false;
                        return;
                    }

                    try
                    {
                        _ = Shell.Current.CurrentPage.Dispatcher.Dispatch(() =>
                        {
                            BallotFolder = ballotPath;
                        });
                        await Task.Run(Upload).ContinueWith((i) =>
                        {
                            _importing = false;
                        });
                    }
                    catch (Exception)
                    {
                        throw;
                    }

                }
            }
            _importing = false;
        });
    }

    public override async Task OnAppearing()
    {
        await base.OnAppearing();
    }

    public override async Task OnLeavingPage()
    {
        _internalManifest?.Dispose();
        _manifestHash?.Dispose();
        _context?.Dispose();

        _timer?.Stop();
        _timer!.Tick -= _timer_Tick;
        await base.OnLeavingPage();
    }
}
