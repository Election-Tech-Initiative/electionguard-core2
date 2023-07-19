using System.Globalization;
using ElectionGuard.Converters;
using Newtonsoft.Json;

namespace ElectionGuard.UI;

public partial class App
{
    public static User CurrentUser { get; set; } = new();
    private ILogger<App> _logger;

    public App(ILogger<App> logger)
    {
        _logger = logger;
        Microsoft.UI.Xaml.Application.Current.UnhandledException += Current_UnhandledException;

        JsonConvert.DefaultSettings = SerializationSettings.NewtonsoftSettings;

        InitializeComponent();
        UserAppTheme = AppTheme.Light;

        SetupLanguageSupport();
        MainPage = new AppShell();
    }

    private void Current_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        _logger.LogError(e.Exception, "Unhandled Exception");
    }

    private void SetupLanguageSupport()
    {
        LocalizationResourceManager.Current.PropertyChanged += CurrentLanguage_Changed;
        LocalizationResourceManager.Current.Init(AppResources.ResourceManager, CultureInfo.CurrentCulture);

        var currentLanguage = Preferences.Get("CurrentLanguage", null);
        LocalizationResourceManager.Current.CurrentCulture = currentLanguage is null ? CultureInfo.CurrentCulture : new CultureInfo(currentLanguage);
    }

    private void CurrentLanguage_Changed(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        AppResources.Culture = LocalizationResourceManager.Current.CurrentCulture;

        // change the first windows title to the new language
        if (Windows.Count > 0)
        {
            Windows[0].Title = GetWindowTitle();
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = base.CreateWindow(activationState);

        if (string.IsNullOrEmpty(window.Title))
        {
            window.Title = GetWindowTitle();
        }

        return window;
    }

    private static string GetWindowTitle()
    {
        return AppResources.WindowTitle;
    }
}
