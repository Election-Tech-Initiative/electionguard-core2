using System.Globalization;
using ElectionGuard.UI.Lib.Services;

namespace ElectionGuard.UI.Services
{
	public class LocalizationService : ILocalizationService
	{
        public string GetValue(string key)
        {
            return LocalizationResourceManager.Current.GetValue(key);
        }

        public string GetLanguage() => Preferences.Get("CurrentLanguage", null) ?? "en";

        public void ToggleLanguage()
        {
            var currentLanguage = GetLanguage();
            if (currentLanguage == "en")
            {
                Preferences.Set("CurrentLanguage", "es");
                LocalizationResourceManager.Current.CurrentCulture = new CultureInfo("es");
            }
            else
            {
                Preferences.Set("CurrentLanguage", "en");
                LocalizationResourceManager.Current.CurrentCulture = new CultureInfo("en");
            }

            OnLanguageChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<EventArgs>? OnLanguageChanged;
    }
}

