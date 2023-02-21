using CommunityToolkit.Mvvm.Input;
using ElectionGuard.UI.Lib.Models;
using ElectionGuard.UI.Lib.Services;
using System.Collections.ObjectModel;

namespace ElectionGuard.UI.ViewModels;

public partial class AdminHomeViewModel : BaseViewModel
{
    private readonly KeyCeremonyService _keyCeremonyService;

    public AdminHomeViewModel(IServiceProvider serviceProvider, KeyCeremonyService keyCeremonyService) : base("AdminHome", serviceProvider)
    {
        _elections.Add(new Election { Name = "Pilot Election" });
        _keyCeremonyService = keyCeremonyService;
    }

    public override async Task OnAppearing()
    {
        var keyCeremonies = await _keyCeremonyService.GetAllNotCompleteAsync();
        if(keyCeremonies is not null)
            KeyCeremonies = new ObservableCollection<KeyCeremony>(keyCeremonies);
    }


    [ObservableProperty]
    private ObservableCollection<Election> _elections = new();

    [ObservableProperty]
    private ObservableCollection<KeyCeremony> _keyCeremonies = new();

    [ObservableProperty]
    private Election? _currentElection;

    [ObservableProperty]
    private KeyCeremony? _currentKeyCeremony;

    [RelayCommand(AllowConcurrentExecutions = true)]
    public async Task GoToKeyCeremony()
    {
        if (CurrentKeyCeremony is null)
            return;
        await NavigationService.GoToPage(typeof(ViewKeyCeremonyViewModel), new Dictionary<string, object>
        {
            { ViewKeyCeremonyViewModel.CurrentKeyCeremonyParam, CurrentKeyCeremony.KeyCeremonyId! }
        });
    }

    [RelayCommand(AllowConcurrentExecutions = true)]
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

    [RelayCommand(AllowConcurrentExecutions = true)]
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
