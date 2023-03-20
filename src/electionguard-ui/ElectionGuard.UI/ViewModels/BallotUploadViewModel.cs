using System.ComponentModel;
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
        BallotUpload upload = new(ElectionId, DeviceFile, deviceData, location, ballots.LongLength, 0, 0, 0, UserName);

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
