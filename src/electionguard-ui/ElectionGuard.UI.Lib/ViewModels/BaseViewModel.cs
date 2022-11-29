using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;
using ElectionGuard.UI.Lib.Models;
using ElectionGuard.UI.Lib.Services;
using System.Globalization;

namespace ElectionGuard.UI.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        private string? _userName;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SettingsCommand))]
        private ContentPage? _page;
        private ILocalizationService _localizationService;
        private INavigationService _navigationService;

        public string Version => $"v{VersionTracking.CurrentVersion}";

        public BaseViewModel(ILocalizationService localization, INavigationService navigation)
        {
            _localizationService = localization;
            _navigationService = navigation;
        }


        [RelayCommand]
        async Task Logout() => await _navigationService.GotoPage(typeof(LoginViewModel));

        [RelayCommand]
        void ChangeLanguage() => _localizationService.Set();

        [RelayCommand(CanExecute = nameof(CanChangeSettings))]
        private void Settings() => _navigationService.GotoModal(typeof(SettingsViewModel));

        private bool CanChangeSettings() => _navigationService.GetCurrentViewModel() is LoginViewModel;

        [RelayCommand(CanExecute = nameof(CanGoHome))]
        private async Task Home() => await _navigationService.GoHome();
        //{
        //    if(App.CurrentUser.IsAdmin)
        //    {
        //        await Shell.Current.GoToAsync($"//{nameof(AdminHomePage)}");
        //    }
        //    else
        //    {
        //        await Shell.Current.GoToAsync($"//{nameof(GuardianHomePage)}");
        //    }
        //}

        private bool CanGoHome() => _navigationService.CanGoHome();
        //{
        //    return (Page is not null) && 
        //        (Page is not LoginPage) && 
        //        (Page is not AdminHomePage) && 
        //        (Page is not GuardianHomePage);
        //}

    }
}
