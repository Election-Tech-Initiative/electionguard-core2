using CommunityToolkit.Mvvm.Input;
using ElectionGuard.UI.Lib.Extensions;

namespace ElectionGuard.UI.Lib.ViewModels
{
    public partial class BaseViewModel : ObservableObject, IDisposable
    {
        [ObservableProperty]
        private string _version;

        [ObservableProperty]
        private string? _userName;

        [ObservableProperty]
        private string _pageTitle = "";

        [RelayCommand]
        private async Task Logout() => await NavigationService.GoToPage(typeof(LoginViewModel));

        [RelayCommand]
        private void ChangeLanguage() => LocalizationService.ToggleLanguage();

        [RelayCommand(CanExecute = nameof(CanChangeSettings))]
        private void Settings() => NavigationService.GoToModal(typeof(SettingsViewModel));

        [RelayCommand(CanExecute = nameof(CanGoHome))]
        private async Task Home() => await NavigationService.GoHome();

        public BaseViewModel(
            string? pageTitleLocalizationId,
            IServiceProvider serviceProvider)
        {
            _pageTitleLocalizationId = pageTitleLocalizationId;
            ConfigurationService = serviceProvider.GetInstance<IConfigurationService>();
            LocalizationService = serviceProvider.GetInstance<ILocalizationService>();
            NavigationService = serviceProvider.GetInstance<INavigationService>();
            AuthenticationService = serviceProvider.GetInstance<IAuthenticationService>();

            _version = ConfigurationService.GetVersion();
            _userName = AuthenticationService.UserName;
            SetPageTitle();

            LocalizationService.OnLanguageChanged += OnLanguageChanged;
        }

        public virtual void Dispose()
        {
            LocalizationService.OnLanguageChanged -= OnLanguageChanged;
            GC.SuppressFinalize(this);
        }

        protected void SetPageTitle()
        {
            if (_pageTitleLocalizationId != null)
            {
                PageTitle = LocalizationService.GetValue(_pageTitleLocalizationId);
            }
        }

        protected IConfigurationService  ConfigurationService { get; }

        protected ILocalizationService LocalizationService { get; }

        protected INavigationService NavigationService { get; }

        protected IAuthenticationService AuthenticationService { get; }

        protected virtual void OnLanguageChanged(object? sender, EventArgs eventArgs) => SetPageTitle();

        private readonly string? _pageTitleLocalizationId;

        private bool CanChangeSettings() => NavigationService.GetCurrentViewModel() == typeof(LoginViewModel);

        private bool CanGoHome() => NavigationService.CanGoHome();
    }
}
