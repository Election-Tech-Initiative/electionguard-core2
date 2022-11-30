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
            NavigationService = serviceProvider.GetInstance<INavigationService>();
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

        protected readonly INavigationService NavigationService;

        protected IAuthenticationService AuthenticationService { get; set; }

        private readonly string? _pageTitleLocalizationId;

        [RelayCommand]
        public async Task Logout() => await NavigationService.GoToPage(typeof(LoginViewModel));

        [RelayCommand]
        public void ChangeLanguage() => LocalizationService.ToggleLanguage();

        protected virtual void OnLanguageChanged(object? sender, EventArgs eventArgs) => SetPageTitle();

        [RelayCommand(CanExecute = nameof(CanChangeSettings))]
        private void Settings() => NavigationService.GoToModal(typeof(SettingsViewModel));

        private bool CanChangeSettings() => NavigationService.GetCurrentViewModel() == typeof(LoginViewModel);

        [RelayCommand(CanExecute = nameof(CanGoHome))]
        private async Task Home() => await NavigationService.GoHome();

        private bool CanGoHome() => NavigationService.CanGoHome();

        public virtual void Dispose()
        {
            LocalizationService.OnLanguageChanged -= OnLanguageChanged;
        }
    }
}
