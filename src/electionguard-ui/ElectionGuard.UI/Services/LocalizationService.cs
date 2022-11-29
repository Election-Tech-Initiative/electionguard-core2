using System;
using System.Globalization;
using ElectionGuard.UI.Lib.Services;

namespace ElectionGuard.UI.Services
{
	public class LocalizationService : ILocalizationService
	{
        public string Get() => Preferences.Get("CurrentLanguage", null) ?? "en";

        public void Set()
        {
            var currentLanguage = Get();
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
        }
    }
}

