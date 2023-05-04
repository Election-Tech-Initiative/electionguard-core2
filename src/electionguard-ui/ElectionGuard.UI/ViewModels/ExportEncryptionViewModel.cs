using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;

namespace ElectionGuard.UI.ViewModels
{
    public partial class EncryptionPackageExportViewModel
        : BaseViewModel
    {
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

        public EncryptionPackageExportViewModel(
            IServiceProvider serviceProvider) 
            : base(null, serviceProvider)
        {
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
        private void Export() { }

        [RelayCommand]
        private void Cancel() { }

        [RelayCommand]
        private void Auto() { }
    }
}
