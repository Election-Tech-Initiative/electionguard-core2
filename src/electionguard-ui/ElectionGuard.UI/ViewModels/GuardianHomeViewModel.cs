namespace ElectionGuard.UI.ViewModels;

public partial class GuardianHomeViewModel : BaseViewModel
{
    private readonly KeyCeremonyService _keyCeremonyService;
    private readonly GuardianPublicKeyService _guardianService;
    private readonly TallyService _tallyService;
    private readonly TallyJoinedService _tallyJoinedService;

    public GuardianHomeViewModel(IServiceProvider serviceProvider, 
        KeyCeremonyService keyCeremonyService,
        GuardianPublicKeyService guardianService,
        TallyService tallyService,
        TallyJoinedService tallyJoinedService) : base("GuardianHome", serviceProvider)
    {
        _keyCeremonyService = keyCeremonyService;
        _guardianService = guardianService;
        _tallyService = tallyService;
        _tallyJoinedService = tallyJoinedService;
    }

    public override async Task OnAppearing()
    {
        await base.OnAppearing();

        _timer.Tick += PollingTimer_Tick;
        _timer.Start();

        PollingTimer_Tick(this, null);
    }

    [ObservableProperty]
    private ObservableCollection<KeyCeremonyRecord> _keyCeremonies = new();

    [ObservableProperty]
    private ObservableCollection<TallyRecord> _tallies = new();

    [ObservableProperty]
    private KeyCeremonyRecord? _currentKeyCeremony;

    [ObservableProperty]
    private TallyRecord? _currentTally;


    public override async Task OnLeavingPage()
    {
        _timer.Stop();
        await Task.Yield();
    }

    partial void OnCurrentKeyCeremonyChanged(KeyCeremonyRecord? value)
    {
        if (value == null)
        {
            return;
        }

        MainThread.BeginInvokeOnMainThread(async() =>
            await NavigationService.GoToPage(typeof(ViewKeyCeremonyViewModel), new Dictionary<string, object>
            {
                { "KeyCeremonyId", value.KeyCeremonyId! }
            }));
    }

    partial void OnCurrentTallyChanged(TallyRecord? value)
    {
        if (value == null)
        {
            return;
        }

        MainThread.BeginInvokeOnMainThread(async () =>
            await NavigationService.GoToPage(typeof(TallyProcessViewModel), new Dictionary<string, object>
            {
                { "TallyId", value.TallyId! }
            }));
    }

    private async void PollingTimer_Tick(object? sender, EventArgs e)
    {
        var keyCeremonies = await _keyCeremonyService.GetAllNotCompleteAsync();
        KeyCeremonies.Clear();
        foreach (var item in keyCeremonies)
        {
            KeyCeremonies.Add(item);
        }

        var tallies = await _tallyService.GetAllByKeyCeremoniesAsync(await _guardianService.GetKeyCeremonyIdsAsync(UserName!));
        var rejected = await _tallyJoinedService.GetGuardianRejectedIdsAsync(UserName!);
        Tallies.Clear();
        foreach (var item in tallies)
        {
            if (!rejected.Contains(item.TallyId) && item.State < TallyState.Complete)
            {
                Tallies.Add(item);
            }
        }
    }
}
