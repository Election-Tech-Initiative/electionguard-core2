using CommunityToolkit.Mvvm.Input;

namespace ElectionGuard.UI.Lib.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _pageTitle = "";

        public IConfigurationService ConfigurationService { get; }

        protected readonly ILocalizationService  LocalizationService;
        
        protected readonly INavigationService NavigationServiceService;

        [ObservableProperty]
        private string? _userName;

        public string Version => ConfigurationService.GetVersion();

        public BaseViewModel(ILocalizationService localization, INavigationService navigation, IConfigurationService configurationService)
        {
            ConfigurationService = configurationService;
            LocalizationService = localization;
            NavigationServiceService = navigation;
        }

        [RelayCommand]
        async Task Logout() => await NavigationServiceService.GoToPage(typeof(LoginViewModel));

        [RelayCommand]
        void ChangeLanguage() => LocalizationService.Set();

        [RelayCommand(CanExecute = nameof(CanChangeSettings))]
        private void Settings() => NavigationServiceService.GoToModal(typeof(SettingsViewModel));

        private bool CanChangeSettings() => NavigationServiceService.GetCurrentViewModel() == typeof(LoginViewModel);

        [RelayCommand(CanExecute = nameof(CanGoHome))]
        private async Task Home() => await NavigationServiceService.GoHome();

        private bool CanGoHome() => NavigationServiceService.CanGoHome();

    }
}
