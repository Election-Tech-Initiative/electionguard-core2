namespace ElectionGuard.UI.ViewModels
{
    public partial class GuardianHomeViewModel : BaseViewModel
    {
        [ObservableProperty]
        private ObservableCollection<KeyCeremony> _keyCeremonies = new();

        [ObservableProperty]
        private ObservableCollection<Tally> _tallies = new();

        [ObservableProperty]
        private KeyCeremony? _currentKeyCeremony;

        [ObservableProperty]
        private Tally? _currentTally;

        public GuardianHomeViewModel()
        {
            // create some fake tallies to add to the list
            _tallies.Add(new Tally { Name = "Election Test Tally #1" });
            _tallies.Add(new Tally { Name = "Election Test Tally #2" });
            _tallies.Add(new Tally { Name = "Real Election Tally" });
            _keyCeremonies.Add(new KeyCeremony { Name="my key" });
        }

    }
}
