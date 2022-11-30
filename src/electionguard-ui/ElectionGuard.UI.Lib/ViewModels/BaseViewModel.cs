using CommunityToolkit.Mvvm.Input;

namespace ElectionGuard.UI.Lib.ViewModels
{
    public partial class BaseViewModel : ObservableObject, IDisposable
    {
        public string Version => ConfigurationService.GetVersion();

        public string? UserName => AuthenticationService.UserName;

        [ObservableProperty]
        private string _pageTitle = "";

        private T GetService<T>(IServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetService(typeof(T));
            if (service == null)
            {
                throw new ArgumentNullException($"{nameof(T)} is not registered with DI");
            }
            return (T)service;
        }

        public BaseViewModel(
            string? pageTitleLocalizationId,
            IServiceProvider serviceProvider)
        {
            _pageTitleLocalizationId = pageTitleLocalizationId;
            ConfigurationService = GetService<IConfigurationService>(serviceProvider);
            LocalizationService = GetService<ILocalizationService>(serviceProvider);
            NavigationServiceService = GetService<INavigationService>(serviceProvider);
            AuthenticationService = GetService<IAuthenticationService>(serviceProvider);

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
