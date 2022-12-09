using System.Globalization;
using ElectionGuard.UI.Lib.Services;

namespace ElectionGuard.UI.Services;

public class LocalizationService : ILocalizationService
{
    public string GetValue(string key) => LocalizationResourceManager.Current.GetValue(key);

    public string GetLanguage() => Preferences.Get("CurrentLanguage", null) ?? "en";

    public void ToggleLanguage()
    {
        var currentLanguage = GetLanguage();
        var newLanguage = currentLanguage == "es" ? "en" : "es";
        Preferences.Set("CurrentLanguage", newLanguage);
        LocalizationResourceManager.Current.CurrentCulture = new CultureInfo(newLanguage);

        OnLanguageChanged?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler<EventArgs>? OnLanguageChanged;
}
