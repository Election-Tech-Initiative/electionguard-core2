using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using ElectionGuard.UI.Lib.Extensions;


namespace ElectionGuard.UI.ViewModels;

public partial class BaseViewModel : ObservableObject, IDisposable
{
    [ObservableProperty]
    private string _appVersion;

    [ObservableProperty]
    private string? _userName;

    [ObservableProperty]
    private bool _isAdmin;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = String.Empty;

    [ObservableProperty]
    private string _pageTitle = "";

    protected IDispatcherTimer? _timer;
    protected ILogger? _logger;

    private void InitTimer()
    {
        _timer = Dispatcher.GetForCurrentThread()?.CreateTimer();
        if (_timer is not null)
        {
            _timer.Interval = TimeSpan.FromSeconds(UISettings.LONG_POLLING_INTERVAL);
            _timer.IsRepeating = true;
        }
    }

    public virtual async Task OnAppearing()
    {
        SetPageTitle();
        LocalizationService.OnLanguageChanged += OnLanguageChanged;

        _timer?.Start();

        await Task.Yield();
    }

    public virtual async Task OnLeavingPage()
    {
        LocalizationService.OnLanguageChanged -= OnLanguageChanged;
        _timer?.Stop();
        await Task.Yield();
    }

    [RelayCommand]
    protected async Task Logout()
    {
        if (Shell.Current.CurrentPage as LoginPage is null)
        {
            _logger?.LogInformation("Logging out as {UserName}", UserName);
            await OnLeavingPage();
            await Shell.Current.CurrentPage.Dispatcher.DispatchAsync(async () =>
            await NavigationService.GoToPage(typeof(LoginViewModel)));
            Dispose();
        }
    }

    [RelayCommand]
    private void ChangeLanguage() => LocalizationService.ToggleLanguage();

    [RelayCommand(CanExecute = nameof(CanChangeSettings), AllowConcurrentExecutions = true)]
    protected async Task Settings()
    {
        await NavigationService.GoToModal(typeof(SettingsViewModel));
    }

    [RelayCommand(CanExecute = nameof(CanGoHome))]
    private async Task Home()
    {
        await OnLeavingPage();
        await NavigationService.GoHome();
    }

    [RelayCommand]
    private async Task GoBack()
    {
        await OnLeavingPage();
        await NavigationService.GoBack();
    }

    /// <summary>
    ///   Constructor for BaseViewModel. 
    /// </summary>
    /// <param name="pageTitleLocalizationId">Page title</param>
    /// <param name="serviceProvider">IOC container</param>
    /// <param name="logger">Logger. Note, BaseViewModel is default logger and may incorrectly show in logs.</param>
    private BaseViewModel(
        string? pageTitleLocalizationId,
        IServiceProvider serviceProvider = null,
        ILogger logger = null) : this(pageTitleLocalizationId, logger)
    {
    }

    public BaseViewModel(
        string? pageTitleLocalizationId,
        ILogger logger)
    {
        _pageTitleLocalizationId = pageTitleLocalizationId;
        _logger = logger;

        ConfigurationService = Ioc.Default.GetService<IConfigurationService>();
        LocalizationService = Ioc.Default.GetService<ILocalizationService>();
        NavigationService = Ioc.Default.GetService<INavigationService>();
        AuthenticationService = Ioc.Default.GetService<IAuthenticationService>();
        AppVersion = ConfigurationService.GetVersion();
        UserName = AuthenticationService.UserName;
        IsAdmin = AuthenticationService.IsAdmin;

        InitTimer();
    }

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    protected void SetPageTitle()
    {
        if (_pageTitleLocalizationId != null)
        {
            PageTitle = LocalizationService.GetValue(_pageTitleLocalizationId);
        }
    }

    private bool IsDatabaseAvailable() => DbService.Ping();

    private bool IsDatabaseUnavailable() => !IsDatabaseAvailable();


    protected IConfigurationService ConfigurationService { get; }

    protected ILocalizationService LocalizationService { get; }

    protected INavigationService NavigationService { get; }

    protected IAuthenticationService AuthenticationService { get; }

    protected virtual void OnLanguageChanged(object? sender, EventArgs eventArgs) => SetPageTitle();

    private readonly string? _pageTitleLocalizationId;

    private bool CanChangeSettings() => NavigationService.GetCurrentViewModel() == typeof(LoginViewModel);

    private bool CanGoHome() => NavigationService.CanGoHome();
}
