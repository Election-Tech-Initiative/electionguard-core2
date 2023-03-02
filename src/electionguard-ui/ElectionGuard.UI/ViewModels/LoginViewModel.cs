using CommunityToolkit.Mvvm.Input;

namespace ElectionGuard.UI.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    public LoginViewModel(IServiceProvider serviceProvider) : base("UserLogin", serviceProvider)
    {
        _timer.Tick += HandleDbPing;
        _timer.Start();
    }

    ~LoginViewModel()
    {
        _timer.Tick -= HandleDbPing;

        if (_timer.IsRunning)
        {
            _timer.Stop();
        }
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _name = string.Empty;

    [ObservableProperty]
    private bool _dbNotAvailable;

    [RelayCommand(CanExecute = nameof(CanLogin), AllowConcurrentExecutions = true)]
    public async Task Login()
    {
        await AuthenticationService.Login(Name);
        // reset the UI name field
        Name = string.Empty;
    }

    private bool CanLogin()
    {
        HandleDbPing(this, EventArgs.Empty);
        return NameHasData;
    }

    private bool NameHasData => Name.Trim().Length > 0;

    public override async Task OnAppearing()
    {
        _timer.Start();
        await base.OnAppearing();
    }

    public override async Task OnLeavingPage()
    {
        _timer.Stop();
        await base.OnLeavingPage();
    }

    private void HandleDbPing(object sender, EventArgs e)
    {
        if (NameHasData)
        {
            DbNotAvailable = !DbService.Ping();
        }
    }
}
