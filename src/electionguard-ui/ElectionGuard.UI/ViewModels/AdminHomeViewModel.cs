using CommunityToolkit.Mvvm.Input;

namespace ElectionGuard.UI.ViewModels;

public partial class AdminHomeViewModel : BaseViewModel
{
    private readonly KeyCeremonyService _keyCeremonyService;
    private readonly ElectionService _electionService;

    public AdminHomeViewModel(IServiceProvider serviceProvider, KeyCeremonyService keyCeremonyService, ElectionService electionService) : base("AdminHome", serviceProvider)
    {
        _keyCeremonyService = keyCeremonyService;
        _electionService = electionService;
    }

    public override async Task OnAppearing()
    {
        var keyCeremonies = await _keyCeremonyService.GetAllNotCompleteAsync();
        if (keyCeremonies is not null)
        {
            KeyCeremonies = new ObservableCollection<KeyCeremonyRecord>(keyCeremonies);
        }

        var elections = await _electionService.GetAllAsync();
        if (elections is not null)
        {
            Elections = new ObservableCollection<Election>(elections);
        }
    }


    [ObservableProperty]
    private ObservableCollection<Election> _elections = new();

    [ObservableProperty]
    private ObservableCollection<KeyCeremonyRecord> _keyCeremonies = new();

    [ObservableProperty]
    private Election? _currentElection;

    [ObservableProperty]
    private KeyCeremonyRecord? _currentKeyCeremony;

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

    [RelayCommand]
    private async Task CreateElection()
    {
        await NavigationService.GoToPage(typeof(CreateElectionViewModel));
    }

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
