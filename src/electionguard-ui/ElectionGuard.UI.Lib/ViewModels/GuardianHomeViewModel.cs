using CommunityToolkit.Mvvm.Input;
using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.Lib.ViewModels
{
    public partial class GuardianHomeViewModel : BaseViewModel
    {
        public GuardianHomeViewModel(IServiceProvider serviceProvider, IKeyCeremonyService keyCeremonyService) : base("GuardianHome", serviceProvider)
        {
            // create some fake tallies to add to the list
            _tallies.Add(new Tally { Name = "Election Test Tally #1" });
            _tallies.Add(new Tally { Name = "Election Test Tally #2" });
            _tallies.Add(new Tally { Name = "Real Election Tally" });

            // todo: database access should not occur in a constructor, create a common async Task lifecycle event in BaseViewModel and override and move this logic there
            var keyCeremonies = keyCeremonyService.List();
            _keyCeremonies = new ObservableCollection<KeyCeremony>(keyCeremonies);
        }

        [ObservableProperty]
        private ObservableCollection<KeyCeremony> _keyCeremonies;

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
 