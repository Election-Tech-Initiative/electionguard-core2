using System.Globalization;

namespace ElectionGuard.UI;

public partial class App
{
    public static User CurrentUser { get; set; } = new();

    public App()
    {
        InitializeComponent();
        UserAppTheme = AppTheme.Light;

        SetupLanguageSupport();

        MainPage = new AppShell();
    }

    private void SetupLanguageSupport()
    {
        LocalizationResourceManager.Current.PropertyChanged += CurrentLanguage_Changed;
        LocalizationResourceManager.Current.Init(AppResources.ResourceManager);

        string? currentLanguage = Preferences.Get("CurrentLanguage", null);
        LocalizationResourceManager.Current.CurrentCulture = currentLanguage is null ? CultureInfo.CurrentCulture : new CultureInfo(currentLanguage);
    }

    private void CurrentLanguage_Changed(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        AppResources.Culture = LocalizationResourceManager.Current.CurrentCulture;

        // change the first windows title to the new language
        if (Windows.Count > 0)
            Windows[0].Title = GetWindowTitle();
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

    private string GetWindowTitle() => AppResources.WindowTitle;
}
