namespace ElectionGuard.UI.Lib.Services
{
	public interface ILocalizationService
	{
        string GetValue(string key);
        public string GetLanguage();
        public void ToggleLanguage();
        event EventHandler<EventArgs> OnLanguageChanged;
    }
}

