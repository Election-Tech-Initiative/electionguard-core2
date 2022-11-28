using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectionGuard.UI.ViewModels
{
    public partial class GuardianHomeViewModel : BaseViewModel
    {
        [ObservableProperty]
        private ObservableCollection<KeyCeremony> keyCeremonies = new();

        [ObservableProperty]
        private ObservableCollection<Tally> tallies = new();

        [ObservableProperty]
        private KeyCeremony? currentKeyCeremony;

        [ObservableProperty]
        private Tally? currentTally;

        public GuardianHomeViewModel()
        {
            // create some fake tallies to add to the list
            tallies.Add(new Tally { Name = "Election Test Tally #1" });
            tallies.Add(new Tally { Name = "Election Test Tally #2" });
            tallies.Add(new Tally { Name = "Real Election Tally" });
            keyCeremonies.Add(new KeyCeremony { Name="my key" });
        }

    }
}
