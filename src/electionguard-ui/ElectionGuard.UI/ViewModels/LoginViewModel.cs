using CommunityToolkit.Mvvm.Input;

namespace ElectionGuard.UI.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    public LoginViewModel(IServiceProvider serviceProvider) : base("UserLogin", serviceProvider)
    {
        SubscribeDbPing();
        HandleDbPing(this, EventArgs.Empty);
    }

    ~LoginViewModel() => UnsubscribeDbPing();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _name = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private bool _dbNotAvailable;

    [RelayCommand(CanExecute = nameof(CanLogin), AllowConcurrentExecutions = true)]
    public async Task Login()
    {
        await AuthenticationService.Login(Name);
        HomeCommand.Execute(this);
        // reset the UI name field
        Name = string.Empty;
    }

    private bool CanLogin()
    {
        return NameHasData && !DbNotAvailable;
    }

    private bool NameHasData => Name.Trim().Length > 0;

    public override async Task OnAppearing()
    {
        SubscribeDbPing();
        await base.OnAppearing();
    }

    private void SubscribeDbPing()
    {
        if (!_timer.IsRunning)
        {
            _timer.Tick += HandleDbPing;
            _timer.Start();
        }
    }

    private void UnsubscribeDbPing()
    {
        if (_timer.IsRunning)
        {
            _timer.Stop();
        }
        _timer.Tick -= HandleDbPing;
    }

    public override async Task OnLeavingPage()
    {
        UnsubscribeDbPing();
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
