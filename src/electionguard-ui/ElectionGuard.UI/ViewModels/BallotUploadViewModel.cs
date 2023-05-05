using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.Input;

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
    private string _resultsText;

    [ObservableProperty]
    private string _uploadText;

    private uint _serialNumber;

    private ElementModQ _manifestHash;


    partial void OnElectionIdChanged(string value)
    {
        Task.Run(async() =>
        {
            var record = await _manifestService.GetByElectionIdAsync(value);
            using var manifest = new Manifest(record.ManifestData);
            _manifestHash = new(manifest.CryptoHash());
        });
    }

    [RelayCommand]
    private void Manual()
    {
        ShowPanel = BallotUploadPanel.ManualUpload;
        _timer.Stop();
    }

    [RelayCommand]
    private void Auto()
    {
        ShowPanel = BallotUploadPanel.AutoUpload;
        _timer.Start();
    }

    [RelayCommand]
    private async Task Cancel()
    {
        await NavigationService.GoToPage(typeof(ElectionViewModel));
    }

    [RelayCommand(CanExecute = nameof(CanUpload))]
    private async Task Upload()
    {
        // create the device file
        var deviceData = File.ReadAllText(DeviceFile, System.Text.Encoding.UTF8);
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
            BallotImported = 0,
            BallotSpoiled = 0,
            BallotDuplicated = 0,
            BallotRejected = 0,
            SerialNumber = _serialNumber,
            CreatedBy = UserName
        };

        long totalCount = 0;
        long totalInserted = 0;
        long totalDuplicated = 0;
        long totalRejected = 0;
        long totalSpoiled = 0;
        ulong startDate = ulong.MaxValue;
        ulong endDate = ulong.MinValue;

        _ = Parallel.ForEach(ballots, (currentBallot) =>
        {
            try
            {
                var filename = Path.GetFileName(currentBallot);
                var ballotData = File.ReadAllText(currentBallot);
                CiphertextBallot ballot = new(ballotData);

                if (ballot.ManifestHash != _manifestHash)
                {
                    _ = Interlocked.Increment(ref totalRejected);
                    return;
                }

                var exists = _ballotService.BallotExists(ballot.BallotCode.ToHex()).Result;
                if (!exists)
                {
                    var timestamp = ballot.Timestamp;
                    if (timestamp < startDate)
                    {
                        _ = Interlocked.Exchange(ref startDate, timestamp);
                    }
                    if (timestamp > endDate)
                    {
                        _ = Interlocked.Exchange(ref endDate, timestamp);
                    }
                    BallotRecord ballotRecord = new()
                    {
                        ElectionId = ElectionId,
                        TimeStamp = DateTime.UnixEpoch.AddSeconds(timestamp),
                        UploadId = upload.UploadId,
                        FileName = filename,
                        BallotCode = ballot.BallotCode.ToHex(),
                        BallotState = ballot.State,
                        BallotData = ballotData
                    };
                    _ = _ballotService.SaveAsync(ballotRecord).Result;

                    if (ballot.State == BallotBoxState.Spoiled)
                    {
                        _ = Interlocked.Increment(ref totalSpoiled);
                    }
                    else
                    {
                        _ = Interlocked.Increment(ref totalInserted);
                    }

                }
                else
                {
                    _ = Interlocked.Increment(ref totalDuplicated);
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
        upload.BallotImported = totalInserted;
        upload.BallotDuplicated = totalDuplicated;
        upload.BallotRejected = totalRejected;
        upload.BallotSpoiled = totalSpoiled;
        upload.BallotsStart = DateTime.UnixEpoch.AddSeconds(startDate);
        upload.BallotsEnd = DateTime.UnixEpoch.AddSeconds(endDate);

        try
        {
            _ = await _uploadService.SaveAsync(upload);
            _timer.Stop();
            ResultsText = $"{AppResources.SuccessText} {totalCount} {AppResources.Success2Text}";
            ShowPanel = BallotUploadPanel.Results;
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
        var customFileType = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI, new[] { ".json" } }, // file extension
                    { DevicePlatform.macOS, new[] { "json" } }, // UTType values
                });
        var options = new PickOptions() { FileTypes = customFileType, PickerTitle = AppResources.SelectManifest };
        FileErrorMessage = string.Empty;
        var file = await FilePicker.PickAsync(options);
        if (file == null)
        {
            FileErrorMessage = "No file was picked";
            // if the picking was canceled then do not change anything
            return;
        }
        // check if it is a device file
        try
        {
            var data = File.ReadAllText(file.FullPath, Encoding.UTF8);
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
            BallotFolder = folder.Folder.Path;
            BallotFolderName = folder.Folder.Name;
            FolderErrorMessage = string.Empty;
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
        _timer.Start();
    }

    private readonly BallotUploadService _uploadService;
    private readonly BallotService _ballotService;
    private readonly ManifestService _manifestService;
    private bool _importing = false;
    private long _lastDrive = -1;

    public BallotUploadViewModel(IServiceProvider serviceProvider, BallotUploadService uploadService, BallotService ballotService, ManifestService manifestService) : base("BallotUploadText", serviceProvider)
    {
        _uploadService = uploadService;
        _ballotService = ballotService;
        _manifestService = manifestService;

        _timer.Tick += _timer_Tick;
        _timer.Start();
    }

#if WINDOWS
    [Flags]
    public enum FileSystemFeature : uint
    {
        /// <summary>
        /// The file system preserves the case of file names when it places a name on disk.
        /// </summary>
        CasePreservedNames = 2,

        /// <summary>
        /// The file system supports case-sensitive file names.
        /// </summary>
        CaseSensitiveSearch = 1,

        /// <summary>
        /// The specified volume is a direct access (DAX) volume. This flag was introduced in Windows 10, version 1607.
        /// </summary>
        DaxVolume = 0x20000000,

        /// <summary>
        /// The file system supports file-based compression.
        /// </summary>
        FileCompression = 0x10,

        /// <summary>
        /// The file system supports named streams.
        /// </summary>
        NamedStreams = 0x40000,

        /// <summary>
        /// The file system preserves and enforces access control lists (ACL).
        /// </summary>
        PersistentACLS = 8,

        /// <summary>
        /// The specified volume is read-only.
        /// </summary>
        ReadOnlyVolume = 0x80000,

        /// <summary>
        /// The volume supports a single sequential write.
        /// </summary>
        SequentialWriteOnce = 0x100000,

        /// <summary>
        /// The file system supports the Encrypted File System (EFS).
        /// </summary>
        SupportsEncryption = 0x20000,

        /// <summary>
        /// The specified volume supports extended attributes. An extended attribute is a piece of
        /// application-specific metadata that an application can associate with a file and is not part
        /// of the file's data.
        /// </summary>
        SupportsExtendedAttributes = 0x00800000,

        /// <summary>
        /// The specified volume supports hard links. For more information, see Hard Links and Junctions.
        /// </summary>
        SupportsHardLinks = 0x00400000,

        /// <summary>
        /// The file system supports object identifiers.
        /// </summary>
        SupportsObjectIDs = 0x10000,

        /// <summary>
        /// The file system supports open by FileID. For more information, see FILE_ID_BOTH_DIR_INFO.
        /// </summary>
        SupportsOpenByFileId = 0x01000000,

        /// <summary>
        /// The file system supports re-parse points.
        /// </summary>
        SupportsReparsePoints = 0x80,

        /// <summary>
        /// The file system supports sparse files.
        /// </summary>
        SupportsSparseFiles = 0x40,

        /// <summary>
        /// The volume supports transactions.
        /// </summary>
        SupportsTransactions = 0x200000,

        /// <summary>
        /// The specified volume supports update sequence number (USN) journals. For more information,
        /// see Change Journal Records.
        /// </summary>
        SupportsUsnJournal = 0x02000000,

        /// <summary>
        /// The file system supports Unicode in file names as they appear on disk.
        /// </summary>
        UnicodeOnDisk = 4,

        /// <summary>
        /// The specified volume is a compressed volume, for example, a DoubleSpace volume.
        /// </summary>
        VolumeIsCompressed = 0x8000,

        /// <summary>
        /// The file system supports disk quotas.
        /// </summary>
        VolumeQuotas = 0x20
    }

    [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public extern static bool GetVolumeInformation(
        string rootPathName,
        StringBuilder volumeNameBuffer,
        int volumeNameSize,
        out uint volumeSerialNumber,
        out uint maximumComponentLength,
        out FileSystemFeature fileSystemFlags,
        StringBuilder fileSystemNameBuffer,
        int nFileSystemNameSize);
#endif

    private void _timer_Tick(object sender, EventArgs e)
    {
        if (_importing)
        {
            return;
        }
        _importing = true;

        Task.Run(async () =>
        {
            // check for a usb drive
            var drives = DriveInfo.GetDrives();
            foreach (var drive in drives)
            {
                if (drive.DriveType == DriveType.Removable && drive.VolumeLabel.ToLower() == "egdrive")
                {
                    _serialNumber = 0;
#if WINDOWS
                    StringBuilder f = new(32);
                    StringBuilder f2 = new(32);
                    GetVolumeInformation(drive.Name, f, 32, out _serialNumber, out _, out _, f2, 32);
#endif
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
                            var data = File.ReadAllText(device, System.Text.Encoding.UTF8);
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
                        DeviceFile = null;
                        _importing = false;
                        return;
                    }

                    try
                    {
                        Shell.Current.CurrentPage.Dispatcher.Dispatch(() =>
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

    public override async Task OnLeavingPage()
    {
        _timer.Stop();
        _timer.Tick -= _timer_Tick;
        await base.OnLeavingPage();
    }
}
