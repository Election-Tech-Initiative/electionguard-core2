using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace ElectionGuard.UI.ViewModels
{
    public partial class AdminHomeViewModel : BaseViewModel
    {
        [ObservableProperty]
        private ObservableCollection<Election> elections = new();

        [ObservableProperty]
        private Election? currentElection;

        [RelayCommand]
        private void CreateElection()
        {
        }

        ElectionViewModel electionVm;

        public AdminHomeViewModel(ElectionViewModel vm)
        {
            electionVm = vm;
            elections.Add(new Election { Name = "Pilot Election" });
        }

        [RelayCommand]
        private async Task SelectionChanged()
        {
            if (CurrentElection is not null)
            {
                electionVm.CurrentElection = CurrentElection;
                await Shell.Current.GoToAsync($"{nameof(ElectionPage)}");
                CurrentElection = null;
            }
        }
    }
}
