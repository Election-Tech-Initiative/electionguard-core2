using CommunityToolkit.Mvvm.Input;
using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.Lib.ViewModels
{
    public partial class AdminHomeViewModel : BaseViewModel
    {
        public AdminHomeViewModel(
            ILocalizationService localizationService, 
            INavigationService navigationService,
            IConfigurationService configurationService,
            ElectionViewModel electionViewModel) : base(localizationService, navigationService, configurationService)
        {
            _electionVm = electionViewModel;
            _elections.Add(new Election { Name = "Pilot Election" });
        }

        [ObservableProperty]
        private ObservableCollection<Election> _elections = new();

        [ObservableProperty]
        private Election? _currentElection;

        [RelayCommand]
        public async Task GoKeyCeremony()
        {
            await NavigationServiceService.GoToPage(typeof(AdminHomeViewModel));
        }

        [RelayCommand]
        private void CreateElection()
        {
        }

        private readonly ElectionViewModel _electionVm;

        [RelayCommand]
        private async Task SelectionChanged()
        {
            if (CurrentElection is not null)
            {
                // todo pass parameter don't depend on singletons
                _electionVm.CurrentElection = CurrentElection;
                await NavigationServiceService.GoToPage(typeof(ElectionViewModel));
                CurrentElection = null;
            }
        }
    }
}
