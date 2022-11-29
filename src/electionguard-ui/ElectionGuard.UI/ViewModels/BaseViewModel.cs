using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;
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

        public string Version => $"v{VersionTracking.CurrentVersion}";

        public BaseViewModel()
        {
            App.CurrentUser.PropertyChanged += CurrentUser_PropertyChanged;
            UserName = App.CurrentUser.Name;
        }

        private void CurrentUser_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var user = sender as User;
            UserName = user?.Name;
        }

        [RelayCommand]
        async Task Logout()
        {
            App.CurrentUser.Name = null;
            await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
        }

        [RelayCommand]
        void ChangeLanguage()
        {
            var currentLanguage = Preferences.Get("CurrentLanguage", null) ?? "en";
            if(currentLanguage == "en")
            {
                Preferences.Set("CurrentLanguage", "es");
                LocalizationResourceManager.Current.CurrentCulture = new CultureInfo("es");
            }
            else
            {
                Preferences.Set("CurrentLanguage", "en");
                LocalizationResourceManager.Current.CurrentCulture = new CultureInfo("en");
            }
        }

        [RelayCommand(CanExecute = nameof(CanChangeSettings))]
        void Settings()
        {
            var popup = new SettingsPage();
            Page?.ShowPopup(popup);
        }

        private bool CanChangeSettings()
        {
            return (Page is LoginPage);
        }

        [RelayCommand(CanExecute = nameof(CanGoHome))]
        private async Task Home()
        {
            if(App.CurrentUser.IsAdmin)
            {
                await Shell.Current.GoToAsync($"//{nameof(AdminHomePage)}");
            }
            else
            {
                await Shell.Current.GoToAsync($"//{nameof(GuardianHomePage)}");
            }
        }

        private bool CanGoHome()
        {
            return (Page is not null) && 
                (Page is not LoginPage) && 
                (Page is not AdminHomePage) && 
                (Page is not GuardianHomePage);
        }

    }
}
