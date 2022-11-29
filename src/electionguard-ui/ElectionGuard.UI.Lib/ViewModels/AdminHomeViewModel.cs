using CommunityToolkit.Mvvm.Input;
using ElectionGuard.UI.Lib.Models;


namespace ElectionGuard.UI.ViewModels
{
    public partial class AdminHomeViewModel : BaseViewModel
    {
        [ObservableProperty]
        private ObservableCollection<Election> _elections = new();

        [ObservableProperty]
        private Election? _currentElection;

        [RelayCommand]
        public async Task GoKeyCeremony()
        {
            await Shell.Current.GoToAsync($"//{nameof(AdminHomePage)}");
        }

        [RelayCommand]
        private void CreateElection()
        {
        }

        private readonly ElectionViewModel _electionVm;

        public AdminHomeViewModel(ElectionViewModel vm)
        {
            _electionVm = vm;
            _elections.Add(new Election { Name = "Pilot Election" });
        }

        [RelayCommand]
        private async Task SelectionChanged()
        {
            if (CurrentElection is not null)
            {
                _electionVm.CurrentElection = CurrentElection;
                await Shell.Current.GoToAsync($"{nameof(ElectionPage)}");
                CurrentElection = null;
            }
        }
    }
}
