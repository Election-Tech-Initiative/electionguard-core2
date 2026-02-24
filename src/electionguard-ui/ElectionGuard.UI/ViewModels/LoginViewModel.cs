using CommunityToolkit.Mvvm.Input;

namespace ElectionGuard.UI.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private bool _hasSeenAutoSettingPage = false;
    private IServiceProvider _serviceProvider;

    public LoginViewModel(IServiceProvider serviceProvider, ILogger<LoginViewModel> logger) : base("UserLogin", serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _name = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private bool _dbNotAvailable = true;

    [RelayCommand(CanExecute = nameof(CanLogin), AllowConcurrentExecutions = true)]
    public async Task Login()
    {
        await AuthenticationService.Login(Name, _logger);
        HomeCommand.Execute(this);
        // reset the UI name field
        Name = string.Empty;
    }

    /// <summary>
    ///  Open settings modal if we are not using a connection string and password is unset.
    /// </summary>
    public void OpenSettingsUnsetData()
    {
        if (!DbContext.IsValid())
        {
            _ = Shell.Current.CurrentPage.Dispatcher.DispatchAsync(async () =>
            {
                await NavigationService.GoToModal(typeof(SettingsViewModel));
                _hasSeenAutoSettingPage = true;
            });
        }
    }

    private bool CanLogin()
    {
        return NameHasData && !DbNotAvailable && DbContext.IsValid();
    }

    private bool NameHasData => Name.Trim().Length > 0;

    public override async Task OnAppearing()
    {
        await base.OnAppearing();

        // update the database info right away
        HandleDbPing(null, EventArgs.Empty);
        SubscribeDbPing();
    }

    public override async Task OnLeavingPage()
    {
        UnsubscribeDbPing();
        await base.OnLeavingPage();
    }

    private void SubscribeDbPing()
    {
        _timer!.Tick += HandleDbPing;
        if (!_timer.IsRunning)
        {
            _timer.Start();
        }
    }

    private void UnsubscribeDbPing()
    {
        if (_timer!.IsRunning)
        {
            _timer.Stop();
        }
        _timer.Tick -= HandleDbPing;
    }

    private void HandleDbPing(object? sender, EventArgs e)
    {
        DbNotAvailable = !DbService.Ping();
        ErrorMessage = DbNotAvailable ? AppResources.DatabaseUnavailable : string.Empty;
        if (sender != null && ErrorLog.AppPreviousCrashed())
        {
            ErrorLog.DeleteCrashedFile();
            _ = Shell.Current.CurrentPage.Dispatcher.DispatchAsync(async () =>
            {
                await Shell.Current.CurrentPage.DisplayAlert(AppResources.PreviousCrash, AppResources.ViewLogsText, AppResources.OkText);
            });
        }
    }
}
