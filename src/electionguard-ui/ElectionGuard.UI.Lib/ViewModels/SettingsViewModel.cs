namespace ElectionGuard.UI.Lib.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public SettingsViewModel(
            ILocalizationService localizationService,
            INavigationService navigationService,
            IConfigurationService configurationService) : base(localizationService, navigationService, configurationService)
        {
        }
    }
}
