using CommunityToolkit.Mvvm.Input;

namespace ElectionGuard.UI.ViewModels
{
    public partial class AdminHomeViewModel : BaseViewModel
    {
        [ObservableProperty]
        private ObservableCollection<Election> _elections = new();

        [ObservableProperty]
        private Election? _currentElection;

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
