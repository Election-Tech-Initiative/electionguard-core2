namespace ElectionGuard.UI.ViewModels;

public partial class GuardianHomeViewModel : BaseViewModel
{
    private readonly KeyCeremonyService _keyCeremonyService;

    public GuardianHomeViewModel(IServiceProvider serviceProvider, KeyCeremonyService keyCeremonyService) : base("GuardianHome", serviceProvider)
    {
        _keyCeremonyService = keyCeremonyService;
        // create some fake tallies to add to the list
}

public override async Task OnAppearing()
    {
        _timer.Start();
        PollingTimer_Tick(this, null);
        await base.OnAppearing();
    }

    [ObservableProperty]
    private ObservableCollection<KeyCeremony> _keyCeremonies = new();

    [ObservableProperty]
    private ObservableCollection<TallyRecord> _tallies = new();

    [ObservableProperty]
    private KeyCeremony? _currentKeyCeremony;

    [ObservableProperty]
    private TallyRecord? _currentTally;


    public override async Task OnLeavingPage()
    {
        _timer.Stop();
        await Task.Yield();
    }

    partial void OnCurrentKeyCeremonyChanged(KeyCeremony? value)
    {
        if (CurrentKeyCeremony == null) return;
        MainThread.BeginInvokeOnMainThread(async() =>
            await NavigationService.GoToPage(typeof(ViewKeyCeremonyViewModel), new Dictionary<string, object>
            {
                { "KeyCeremonyId", CurrentKeyCeremony.KeyCeremonyId! }
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
    }


}
