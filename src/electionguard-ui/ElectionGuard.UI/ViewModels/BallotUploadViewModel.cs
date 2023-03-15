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
    }

    [RelayCommand]
    private void Auto()
    {
        ShowWizard = true;
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
        string deviceData = File.ReadAllText(DeviceFile, System.Text.Encoding.UTF8);
        EncryptionDevice device = new(deviceData);

        // save the ballot upload
        var ballots = Directory.GetFiles(BallotFolder);
        BallotUpload upload = new(ElectionId, DeviceFile, deviceData, ballots.LongLength, UserName);

        await _uploadService.SaveAsync(upload);
        
        long totalCount = 0;
        
        Parallel.ForEach<string, long>(ballots,
            () => 0,
            (currentBallot, loop, subtotal) =>
            {
                try
                {
                    string filename = Path.GetFileName(currentBallot);
                    var ballotData = File.ReadAllText(currentBallot);
                    SubmittedBallot ballot = new(ballotData);
                    BallotRecord ballotRecord = new(ElectionId, upload.UploadId, filename, ballotData);
                    _ = _ballotService.SaveAsync(ballotRecord).Result;

                    subtotal += 1;
                }
                catch(Exception ex)
                {

                }
                return subtotal;
            },
            (finalResult) => Interlocked.Add(ref totalCount, finalResult)
            );

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
            string data = File.ReadAllText(file.FullPath, System.Text.Encoding.UTF8);
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
            BallotFolder = folder.Path;
            BallotFolderName = folder.Name;
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

    public BallotUploadViewModel(IServiceProvider serviceProvider, BallotUploadService uploadService, BallotService ballotService) : base("BallotUploadText", serviceProvider)
    {
        _uploadService = uploadService;
        _ballotService = ballotService;
    }
}
