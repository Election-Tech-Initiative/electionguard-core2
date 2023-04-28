using CommunityToolkit.Mvvm.Input;
using ElectionGuard.UI.Models;

namespace ElectionGuard.UI.ViewModels
{
    [QueryProperty(ElectionIdParam, nameof(ElectionId))]
    public partial class EncryptionPackageExportViewModel
        : BaseViewModel
    {
        public const string ElectionIdParam = "ElectionId";

        [ObservableProperty]
        private string _electionId = string.Empty;

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
        private void Export()
        {
            Task.Run(async () => await ExportAsync());
        }

        [RelayCommand]
        private void Cancel() { }

        [RelayCommand]
        private void Auto() { }

        private async Task ExportAsync()
        {
            var package = await FetchEncryptionPackage();
            WriteEncryptionPackage(package);
        }

        private async Task<EncryptionPackage> FetchEncryptionPackage()
        {
            Election? election = await _electionService.GetByElectionIdAsync(ElectionId);

            if (election == null)
            {
                throw new ElectionGuardException(
                    $"{nameof(EncryptionPackageExportViewModel)}::${nameof(ExportAsync)}::{nameof(ElectionId)}::{ElectionId}::Not Found");
            }

            return new EncryptionPackage(
                await _contextService.GetByElectionIdAsync(ElectionId),
                await _constantsService.GetByElectionIdAsync(ElectionId),
                await _manifestService.GetByElectionIdAsync(ElectionId)
            );
        }

        private void WriteEncryptionPackage(EncryptionPackage package)
        {
            if (package == null) throw new ArgumentNullException(nameof(package));

            _storageService.UpdatePath(FileFolder);

            List<FileContents> files = new()
            {
                new FileContents(UISettings.EncryptionPackageFilenames.CONSTANTS,package.Constants),
                new FileContents(UISettings.EncryptionPackageFilenames.CONTEXT, package.Context),
                new FileContents(UISettings.EncryptionPackageFilenames.MANIFEST, package.Manifest)
            };

            _storageService.ToFiles(files);
        }
    }
}
