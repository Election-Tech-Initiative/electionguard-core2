using CommunityToolkit.Mvvm.Input;

namespace ElectionGuard.UI.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private bool _hasSeenAutoSettingPage = false;

    public LoginViewModel(IServiceProvider serviceProvider) : base("UserLogin", serviceProvider)
    {
        SubscribeDbPing();
        HandleDbPing(this, EventArgs.Empty);
    }

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

    /// <summary>
    ///  Open settings modal if we are not using a connection string and password is unset.
    /// </summary>
    private void OpenSettingsUnsetData()
    {
        if (!DbContext.IsValid() && !_hasSeenAutoSettingPage)
        {
            _hasSeenAutoSettingPage = true;
            Settings();
        }
    }

    private bool CanLogin()
    {
        return NameHasData && !DbNotAvailable;
    }

    private bool NameHasData => Name.Trim().Length > 0;

    public override async Task OnAppearing()
    {
        await base.OnAppearing();

        SubscribeDbPing();
    }

    public override async Task OnLeavingPage()
    {
        UnsubscribeDbPing();
        await base.OnLeavingPage();
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

    private void HandleDbPing(object sender, EventArgs e)
    {
        if (!_hasSeenAutoSettingPage)
        {
            OpenSettingsUnsetData();
            return;
        }

        if (NameHasData)
        {
            DbNotAvailable = !DbService.Ping();
        }
    }
}
