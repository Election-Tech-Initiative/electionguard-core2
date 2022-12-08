using CommunityToolkit.Mvvm.Input;
using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.Lib.ViewModels
{
    public partial class GuardianHomeViewModel : BaseViewModel
    {
        private readonly IKeyCeremonyService _keyCeremonyService;

        public GuardianHomeViewModel(IServiceProvider serviceProvider, IKeyCeremonyService keyCeremonyService) : base("GuardianHome", serviceProvider)
        {
            _keyCeremonyService = keyCeremonyService;
            // create some fake tallies to add to the list
            _tallies.Add(new Tally { Name = "Election Test Tally #1" });
            _tallies.Add(new Tally { Name = "Election Test Tally #2" });
            _tallies.Add(new Tally { Name = "Real Election Tally" });
        }

        public override async Task Appearing()
        {
            var keyCeremonies = await _keyCeremonyService.List();
            KeyCeremonies = new ObservableCollection<KeyCeremony>(keyCeremonies);
        }

        [ObservableProperty]
        private ObservableCollection<KeyCeremony> _keyCeremonies = new ();

        [ObservableProperty]
        private ObservableCollection<Tally> _tallies = new();

        [ObservableProperty]
        private KeyCeremony? _currentKeyCeremony;

        [ObservableProperty]
        private Tally? _currentTally;

        [RelayCommand]
        public async Task KeyCeremonySelectionChanged()
        {
            if (CurrentKeyCeremony == null) return;
            await NavigationService.GoToPage(typeof(ViewKeyCeremonyViewModel), new Dictionary<string, object>
            {
                { "KeyCeremonyId", CurrentKeyCeremony.Id }
            });
        }
    }
}
 