using CommunityToolkit.Mvvm.Input;

namespace ElectionGuard.UI.Lib.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly IAuthenticationService _authenticationService;

        public LoginViewModel(
            ILocalizationService localizationService,
            INavigationService navigationService,
            IConfigurationService configurationService,
            IAuthenticationService authenticationService) : base(localizationService, navigationService, configurationService)
        {
            _authenticationService = authenticationService;
        }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginViewModel.LoginCommand))]
        private string _name = string.Empty;

        [RelayCommand(CanExecute = nameof(CanLogin))]
        public async Task Login()
        {
            await _authenticationService.Login(Name);
            // reset the UI name field
            Name = string.Empty;
        }

        bool CanLogin()
        {
            return Name.Trim().Length > 0;
        }
    }
}
