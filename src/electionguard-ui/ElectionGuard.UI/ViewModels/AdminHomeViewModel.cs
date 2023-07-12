using CommunityToolkit.Mvvm.Input;

namespace ElectionGuard.UI.ViewModels;

public partial class AdminHomeViewModel : BaseViewModel
{
    private readonly KeyCeremonyService _keyCeremonyService;
    private readonly ElectionService _electionService;
    private readonly MultiTallyService _multiTallyService;
    private readonly TallyService _tallyService;

    public AdminHomeViewModel(
        IServiceProvider serviceProvider,
        KeyCeremonyService keyCeremonyService,
        ElectionService electionService,
        TallyService tallyService,
        MultiTallyService multiTallyService) : base("AdminHome", serviceProvider)
    {
        _keyCeremonyService = keyCeremonyService;
        _electionService = electionService;
        _tallyService = tallyService;
        _multiTallyService = multiTallyService;
    }

    public override async Task OnAppearing()
    {
        await base.OnAppearing();

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

        MultiTallies.Clear();
        var multiTallies = await _multiTallyService.GetAllAsync();
        foreach (var multiTally in multiTallies)
        {
            var addMultiTally = false;
            
            // check each tally in the multitally to see if any are not complete / abandoned
            foreach (var (tallyId, _, _) in multiTally.TallyIds)
            {
                if (await _tallyService.IsRunningByTallyIdAsync(tallyId))
                {
                    addMultiTally = true;
                    break;
                }
            }
            if (addMultiTally)
            {
                MultiTallies.Add(multiTally);
            }
        }
    }

    [ObservableProperty]
    private ObservableCollection<Election> _elections = new();

    [ObservableProperty]
    private ObservableCollection<KeyCeremonyRecord> _keyCeremonies = new();

    [ObservableProperty]
    private ObservableCollection<MultiTallyRecord> _multiTallies = new();

    [ObservableProperty]
    private Election? _currentElection;

    [ObservableProperty]
    private KeyCeremonyRecord? _currentKeyCeremony;

    [ObservableProperty]
    private MultiTallyRecord? _currentMultiTally;

    partial void OnCurrentMultiTallyChanged(MultiTallyRecord? value)
    {
        if (value == null)
        {
            return;
        }

        MainThread.BeginInvokeOnMainThread(async () =>
            await NavigationService.GoToPage(typeof(TallyProcessViewModel), new()
            {
                { CreateMultiTallyViewModel.MultiTallyIdParam, value.MultiTallyId! }
            }));
    }


    [RelayCommand(AllowConcurrentExecutions = true)]
    public async Task GoToKeyCeremony()
    {
        if (CurrentKeyCeremony is null)
            return;
        await NavigationService.GoToPage(typeof(ViewKeyCeremonyViewModel), new()
        {
            { ViewKeyCeremonyViewModel.CurrentKeyCeremonyParam, CurrentKeyCeremony.KeyCeremonyId! }
        });
    }

    [RelayCommand(AllowConcurrentExecutions = true)]
    public async Task GoKeyCeremony()
    {
        await NavigationService.GoToPage(typeof(CreateKeyCeremonyAdminViewModel));
    }

    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task CreateElection()
    {
        await NavigationService.GoToPage(typeof(CreateElectionViewModel));
    }

    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task SelectionChanged()
    {
        if (CurrentElection is not null)
        {
            await NavigationService.GoToPage(typeof(ElectionViewModel), new()
            {
                { ElectionViewModel.CurrentElectionParam, CurrentElection }
            });
            CurrentElection = null;
        }
    }

    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task CreateMultipleTallies()
    {
        await NavigationService.GoToPage(typeof(CreateMultiTallyViewModel));
    }

}
