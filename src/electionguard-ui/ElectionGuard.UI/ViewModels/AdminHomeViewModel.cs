using CommunityToolkit.Mvvm.Input;
using ElectionGuard.UI.Lib.Models;
using System.Collections.ObjectModel;

namespace ElectionGuard.UI.ViewModels;

public partial class AdminHomeViewModel : BaseViewModel
{
    public AdminHomeViewModel(IServiceProvider serviceProvider) : base("AdminHome", serviceProvider)
    {
        _elections.Add(new Election { Name = "Pilot Election" });
    }

    [ObservableProperty]
    private ObservableCollection<Election> _elections = new();

    [ObservableProperty]
    private Election? _currentElection;

    [RelayCommand]
    public async Task GoKeyCeremony()
    {
        await NavigationService.GoToPage(typeof(CreateKeyCeremonyAdminViewModel));
    }

#pragma warning disable CA1822 // This is a stub for future use
    [RelayCommand]
    private void CreateElection()
    {
    }
#pragma warning restore CA1822

    [RelayCommand]
    private async Task SelectionChanged()
    {
        if (CurrentElection is not null)
        {
            var pageParams = new Dictionary<string, object>
            {
                { ElectionViewModel.CurrentElectionParam, CurrentElection }
            };
            await NavigationService.GoToPage(typeof(ElectionViewModel), pageParams);
            CurrentElection = null;
        }
    }
}
