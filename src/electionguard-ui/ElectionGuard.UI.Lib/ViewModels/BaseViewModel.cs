using CommunityToolkit.Mvvm.Input;
using ElectionGuard.UI.Lib.Extensions;

namespace ElectionGuard.UI.Lib.ViewModels
{
    public partial class BaseViewModel : ObservableObject, IDisposable
    {
        public string Version => ConfigurationService.GetVersion();

        public string? UserName => AuthenticationService.UserName;

        [ObservableProperty]
        private string _pageTitle = "";

        public BaseViewModel(
            string? pageTitleLocalizationId,
            IServiceProvider serviceProvider)
        {
            _pageTitleLocalizationId = pageTitleLocalizationId;
            ConfigurationService = serviceProvider.GetInstance<IConfigurationService>();
            LocalizationService = serviceProvider.GetInstance<ILocalizationService>();
            NavigationServiceService = serviceProvider.GetInstance<INavigationService>();
            AuthenticationService = serviceProvider.GetInstance<IAuthenticationService>();

            SetPageTitle();

            LocalizationService.OnLanguageChanged += OnLanguageChanged;
        }

        protected void SetPageTitle()
        {
            if (_pageTitleLocalizationId != null)
            {
                PageTitle = LocalizationService.GetValue(_pageTitleLocalizationId);
            }
        }

        protected IConfigurationService ConfigurationService { get; }

        protected readonly ILocalizationService  LocalizationService;

        protected readonly INavigationService NavigationServiceService;

        protected IAuthenticationService AuthenticationService { get; set; }

        private readonly string? _pageTitleLocalizationId;

        [RelayCommand]
        public async Task Logout() => await NavigationServiceService.GoToPage(typeof(LoginViewModel));

        [RelayCommand]
        public void ChangeLanguage() => LocalizationService.ToggleLanguage();

        protected virtual void OnLanguageChanged(object? sender, EventArgs eventArgs) => SetPageTitle();

        [RelayCommand(CanExecute = nameof(CanChangeSettings))]
        private void Settings() => NavigationServiceService.GoToModal(typeof(SettingsViewModel));

        private bool CanChangeSettings() => NavigationServiceService.GetCurrentViewModel() == typeof(LoginViewModel);

        [RelayCommand(CanExecute = nameof(CanGoHome))]
        private async Task Home() => await NavigationServiceService.GoHome();

        private bool CanGoHome() => NavigationServiceService.CanGoHome();

        public virtual void Dispose()
        {
            LocalizationService.OnLanguageChanged -= OnLanguageChanged;
        }
    }
}
