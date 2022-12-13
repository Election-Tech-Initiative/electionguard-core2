using CommunityToolkit.Mvvm.Input;

namespace ElectionGuard.UI.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    public LoginViewModel(IServiceProvider serviceProvider) : base("UserLogin", serviceProvider)
    {
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _name = string.Empty;

    [RelayCommand(CanExecute = nameof(CanLogin))]
    public async Task Login()
    {
        await AuthenticationService.Login(Name);
        // reset the UI name field
        Name = string.Empty;
    }

    private bool CanLogin()
    {
        return Name.Trim().Length > 0;
    }
}
