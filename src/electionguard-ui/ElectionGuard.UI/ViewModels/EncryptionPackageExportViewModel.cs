using System.Text;
using CommunityToolkit.Mvvm.Input;
using ElectionGuard.UI.Models;
using ElectionGuard.UI.Helpers;
using System.IO;

namespace ElectionGuard.UI.ViewModels
{
    /// TODO: There has to be a way to better share this view model with <see cref="BallotUploadViewModel" />
    [QueryProperty(ElectionIdParam, nameof(ElectionId))]
    public partial class EncryptionPackageExportViewModel
        : BaseViewModel
    {
        public const string ElectionIdParam = "ElectionId";

        [ObservableProperty]
        private string _electionId = string.Empty;

        [ObservableProperty]
        private EncryptionPackage _package;

        [ObservableProperty]
        private bool _showPanel = false;

        [ObservableProperty]
        private string _uploadText = string.Empty;

        [ObservableProperty]
        private string _fileErrorMessage = string.Empty;

        [ObservableProperty]
        private string _fileFolder = string.Empty;

        [ObservableProperty]
        private string _folderErrorMessage = string.Empty;

        [ObservableProperty]
        private string _resultsText = string.Empty;

        private readonly ElectionService _electionService;
        private readonly ContextService _contextService;
        private readonly ConstantsService _constantsService;
        private readonly ManifestService _manifestService;

        private readonly IStorageService _storageService;

        public EncryptionPackageExportViewModel(
            IServiceProvider serviceProvider,
            ElectionService electionService,
            ContextService contextService,
            ConstantsService constantsService,
            ManifestService manifestService,
            DriveService storageService)
                : base(null, serviceProvider)
        {
            _electionService = serviceProvider.GetRequiredService<ElectionService>();
            _contextService = contextService;
            _constantsService = constantsService;
            _manifestService = manifestService;
            _storageService = storageService;

            _timer.Tick += _timer_Tick;
            _timer.Start();
        }

        partial void OnElectionIdChanged(string value)
        {
            Task.Run(FetchEncryptionPackage);
        }

        [RelayCommand]
        private void Manual()
        {
        }

        [RelayCommand]
        private void PickDeviceFile() { }

        [RelayCommand]
        private void PickFolder() { }

        [RelayCommand]
        private void Export() => ExportAsync("",0);

        [RelayCommand]
        private void Cancel() { }

        [RelayCommand]
        private void Auto() { }

        private void ExportAsync(string fileTarget, uint serial)
        {
            WriteEncryptionPackage(fileTarget, Package);
            DocumentExport(serial);
        }

        private void DocumentExport(uint serial)
        {
            if (serial != 0)
            { 
                exportedDrives.Add(serial);
            }
        }

        private string getFileTarget(DriveInfo driveInfo) => driveInfo.Name;

        private async Task FetchEncryptionPackage()
        {
            Election? election = await _electionService.GetByElectionIdAsync(ElectionId);

            if (election == null)
            {
                throw new ElectionGuardException(
                    $"{nameof(EncryptionPackageExportViewModel)}::${nameof(ExportAsync)}::{nameof(ElectionId)}::{ElectionId}::Not Found");
            }

            var context = await _contextService.GetByElectionIdAsync(ElectionId);
            var constants = await _constantsService.GetByElectionIdAsync(ElectionId);
            var manifest = await _manifestService.GetByElectionIdAsync(ElectionId);

            if (context == null || constants == null || manifest == null)
            {
                throw new ElectionGuardException(
                    $"{nameof(EncryptionPackageExportViewModel)}::${nameof(ExportAsync)}::{nameof(ElectionId)}::{ElectionId}::Encryption Data not created");
            }

            Package = new EncryptionPackage(context, constants, manifest);
        }

        private void WriteEncryptionPackage(string fileFolder, EncryptionPackage package)
        {
            if (package == null) throw new ArgumentNullException(nameof(package));
            if (string.IsNullOrEmpty(fileFolder)) throw new ArgumentNullException(nameof(fileFolder));

            _storageService.UpdatePath(fileFolder);

            List<FileContents> files = new()
            {
                new FileContents(UISettings.EncryptionPackageFilenames.CONSTANTS,package.Constants),
                new FileContents(UISettings.EncryptionPackageFilenames.CONTEXT, package.Context),
                new FileContents(UISettings.EncryptionPackageFilenames.MANIFEST, package.Manifest)
            };

            _storageService.ToFiles(files);
        }

        private bool isRunning;
        private void _timer_Tick(object sender, EventArgs e)
        {
            // HACK: this feels like something that belongs at a higher level, like the timer's scheduler
            if (isRunning)
                return;

            isRunning = true;
            try
            {
                Task.Run(async() => await ExportPackageToDriveAsync());
            }
            finally
            {
                isRunning = false;
            } 
        }

        private List<uint> exportedDrives = new();

        private async Task ExportPackageToDriveAsync()
        {
            // NOTE: This is assumed to be lowercase
            const string egDriveLabel = "egdrive";

            if (Package == null) return;
            
            // check for a usb drive named egDrive
            var allAttachedDrives = DriveInfo.GetDrives();

            var egDrives = from drive in allAttachedDrives
                           where drive != null
                           where drive.DriveType == DriveType.Removable
                           where drive.VolumeLabel.ToLower() == egDriveLabel
                           select drive;

            Parallel.ForEach(egDrives, async egDrive =>
            {
                var serialNumber = 0U;
                _ = StorageUtils.GetVolumeInformation(egDrive.Name, out serialNumber);

                if (!HasDriveReceivedExport(serialNumber))
                {
                    // export
                    ExportAsync(getFileTarget(egDrive), serialNumber);

                    // 
                }
            });

                foreach (var drive in egDrives)
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
                _importing = false;
        }

        private bool HasDriveReceivedExport(uint serialNumber)
        {
            // TODO: support non-windows.
            if (serialNumber == StorageUtils.UnknownSerial)
                return false;

            return exportedDrives.Where(serial => serial == serialNumber).Count() > 0;
        }
    }
}
