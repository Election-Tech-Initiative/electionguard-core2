using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using CommunityToolkit.Maui.Core.Primitives;
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
    private bool _showWizard = true;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UploadCommand))]
    private string _deviceFile = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UploadCommand))]
    private string _ballotFolder = string.Empty;

    [ObservableProperty]
    private string _ballotFolderName = string.Empty;

    private uint _serialNumber;


    [RelayCommand]
    private void Manual()
    {
        ShowWizard = false;
        _timer.Stop();
    }

    [RelayCommand]
    private void Auto()
    {
        ShowWizard = true;
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
        using (var jsonDoc = JsonDocument.Parse(deviceData))
        {
            var dev = jsonDoc.RootElement.GetProperty("location");
            location = dev.GetString();
        }
        // save the ballot upload
        var ballots = Directory.GetFiles(BallotFolder);
        BallotUpload upload = new(ElectionId, DeviceFile, deviceData, location, ballots.LongLength, 0, 0, 0, _serialNumber, UserName);

        long totalCount = 0;

        _ = Parallel.ForEach<string, long>(ballots,
            () => 0,
            (currentBallot, loop, subtotal) =>
            {
                try
                {
                    var filename = Path.GetFileName(currentBallot);
                    var ballotData = File.ReadAllText(currentBallot);
                    SubmittedBallot ballot = new(ballotData);
                    BallotRecord ballotRecord = new(ElectionId, upload.UploadId, filename, ballotData);
                    _ = _ballotService.SaveAsync(ballotRecord).Result;

                    subtotal += 1;
                }
                catch (Exception ex)
                {

                }
                return subtotal;
            },
            (finalResult) => Interlocked.Add(ref totalCount, finalResult)
            );

        // update totals before saving

        _ = await _uploadService.SaveAsync(upload);
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
            var data = File.ReadAllText(file.FullPath, System.Text.Encoding.UTF8);
            EncryptionDevice device = new(data);
            DeviceFile = file.FullPath;
        }
        catch (Exception ex)
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

    private readonly BallotUploadService _uploadService;
    private readonly BallotService _ballotService;
    private bool _importing = false;

    public BallotUploadViewModel(IServiceProvider serviceProvider, BallotUploadService uploadService, BallotService ballotService) : base("BallotUploadText", serviceProvider)
    {
        _uploadService = uploadService;
        _ballotService = ballotService;

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
                    catch (Exception ex)
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

                BallotFolder = ballotPath;

                _ = Task.Run(Upload);

                _importing = false;
            }
        }
    }

    public override async Task OnLeavingPage()
    {
        _timer.Stop();
        _timer.Tick -= _timer_Tick;
        await base.OnLeavingPage();
    }

}
