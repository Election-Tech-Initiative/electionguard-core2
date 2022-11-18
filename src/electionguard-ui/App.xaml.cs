using ElectionGuard.UI.Resx;
using System.Globalization;

namespace ElectionGuard.UI;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        LocalizationResourceManager.Current.PropertyChanged += (_, _) => AppResources.Culture = LocalizationResourceManager.Current.CurrentCulture;
        LocalizationResourceManager.Current.Init(AppResources.ResourceManager);

        string? currentLanguage = Preferences.Get("CurrentLanguage", null);
        LocalizationResourceManager.Current.CurrentCulture = currentLanguage is null ? CultureInfo.CurrentCulture : new CultureInfo(currentLanguage);


        MainPage = new AppShell();
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
