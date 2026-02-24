namespace ElectionGuard.UI.ViewModels;

public partial class GuardianHomeViewModel : BaseViewModel
{
    private readonly KeyCeremonyService _keyCeremonyService;
    private readonly GuardianPublicKeyService _guardianService;
    private readonly TallyService _tallyService;
    private readonly TallyJoinedService _tallyJoinedService;
    private readonly MultiTallyService _multiTallyService;

    public GuardianHomeViewModel(IServiceProvider serviceProvider,
        KeyCeremonyService keyCeremonyService,
        GuardianPublicKeyService guardianService,
        TallyService tallyService,
        TallyJoinedService tallyJoinedService,
        MultiTallyService multiTallyService) : base("GuardianHome", serviceProvider)
    {
        _keyCeremonyService = keyCeremonyService;
        _guardianService = guardianService;
        _tallyService = tallyService;
        _tallyJoinedService = tallyJoinedService;
        _multiTallyService = multiTallyService;
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
    private ObservableCollection<MultiTallyRecord> _multiTallies = new();

    [ObservableProperty]
    private ObservableCollection<TallyRecord> _tallies = new();

    [ObservableProperty]
    private KeyCeremonyRecord? _currentKeyCeremony;

    [ObservableProperty]
    private TallyRecord? _currentTally;

    [ObservableProperty]
    private MultiTallyRecord? _currentMultiTally;


    public override async Task OnLeavingPage()
    {
        _timer.Stop();
        await Task.Yield();
    }

    partial void OnCurrentKeyCeremonyChanged(KeyCeremonyRecord? value)
    {
        if (value is null)
        {
            return;
        }

        MainThread.BeginInvokeOnMainThread(async() =>
            await NavigationService.GoToPage(typeof(ViewKeyCeremonyViewModel), new Dictionary<string, object>
            {
                { ViewKeyCeremonyViewModel.CurrentKeyCeremonyParam, value }
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

    partial void OnCurrentMultiTallyChanged(MultiTallyRecord? value)
    {
        if (value == null)
        {
            return;
        }

        MainThread.BeginInvokeOnMainThread(async () =>
            await NavigationService.GoToPage(typeof(CreateMultiTallyViewModel), new Dictionary<string, object>
            {
                { CreateMultiTallyViewModel.MultiTallyIdParam, value.MultiTallyId! }
            }));
    }

    private async void PollingTimer_Tick(object? sender, EventArgs e)
    {
        try
        {
            var keyCeremonies = await _keyCeremonyService.GetAllNotCompleteAsync();
            KeyCeremonies.Clear();
            foreach (var item in keyCeremonies)
            {
                KeyCeremonies.Add(item);
            }

            var keys = await _guardianService.GetKeyCeremonyIdsAsync(UserName!);
            var tallies = await _tallyService.GetAllByKeyCeremoniesAsync(keys);
            var rejected = await _tallyJoinedService.GetGuardianRejectedIdsAsync(UserName!);
            Tallies.Clear();
            foreach (var item in tallies)
            {
                if (!rejected.Contains(item.TallyId) && item.State < TallyState.Complete)
                {
                    Tallies.Add(item);
                }
            }

            var multiTallies = await _multiTallyService.GetAllAsync();
            foreach (var tally in multiTallies)
            {
                if (!keys.Contains(tally.KeyCeremonyId!))
                {
                    continue;
                }

                var addMulti = false;
                // check each tally in the multitally to see if any are not complete / abandoned
                foreach (var (tallyId, _, _) in tally.TallyIds)
                {
                    if (await _tallyService.IsRunningByTallyIdAsync(tallyId))
                    {
                        addMulti = true;
                        break;
                    }
                }
                if (addMulti)
                {
                    if (MultiTallies.Count(m => m.MultiTallyId == tally.MultiTallyId) == 0)
                    {
                        MultiTallies.Add(tally);
                    }
                }
                else
                {
                    if (MultiTallies.Count(m => m.MultiTallyId == tally.MultiTallyId) > 0)
                    {
                        MultiTallies.Remove(tally);
                    }
                }
            }
        }
        catch(Exception)
        {
            // if we have an exception, do not try to update anymore
            _timer.Stop();
        }
    }
}
