using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.Lib.ViewModels
{
    public partial class GuardianHomeViewModel : BaseViewModel
    {
        public GuardianHomeViewModel(
            ILocalizationService localizationService,
            INavigationService navigationService,
            IConfigurationService configurationService) : base(localizationService, navigationService, configurationService)
        {
            // create some fake tallies to add to the list
            _tallies.Add(new Tally { Name = "Election Test Tally #1" });
            _tallies.Add(new Tally { Name = "Election Test Tally #2" });
            _tallies.Add(new Tally { Name = "Real Election Tally" });
            _keyCeremonies.Add(new KeyCeremony { Name = "my key" });
        }

        [ObservableProperty]
        private ObservableCollection<KeyCeremony> _keyCeremonies = new();

        [ObservableProperty]
        private ObservableCollection<Tally> _tallies = new();

        [ObservableProperty]
        private KeyCeremony? _currentKeyCeremony;

        [ObservableProperty]
        private Tally? _currentTally;
    }
}
