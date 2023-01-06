using CommunityToolkit.Mvvm.Input;
using ElectionGuard.UI.Lib.Models;
using ElectionGuard.UI.Lib.Services;

namespace ElectionGuard.UI.ViewModels;

public partial class GuardianHomeViewModel : BaseViewModel
{
    private readonly KeyCeremonyService _keyCeremonyService;

    public GuardianHomeViewModel(IServiceProvider serviceProvider, KeyCeremonyService keyCeremonyService) : base("GuardianHome", serviceProvider)
    {
        _keyCeremonyService = keyCeremonyService;
        // create some fake tallies to add to the list
        _tallies.Add(new Tally { Name = "Election Test Tally #1" });
        _tallies.Add(new Tally { Name = "Election Test Tally #2" });
        _tallies.Add(new Tally { Name = "Real Election Tally" });
    }

    public override async Task Appearing()
    {
        var keyCeremonies = await _keyCeremonyService.GetAllAsync();
        KeyCeremonies = new ObservableCollection<KeyCeremony>(keyCeremonies);
    }

    [ObservableProperty]
    private ObservableCollection<KeyCeremony> _keyCeremonies = new ();

    [ObservableProperty]
    private ObservableCollection<Tally> _tallies = new();

    [ObservableProperty]
    private KeyCeremony? _currentKeyCeremony;

    [ObservableProperty]
    private Tally? _currentTally;

    partial void OnCurrentKeyCeremonyChanged(KeyCeremony? value)
    {
        if (CurrentKeyCeremony == null) return;
        MainThread.BeginInvokeOnMainThread(async() =>
            await NavigationService.GoToPage(typeof(ViewKeyCeremonyViewModel), new Dictionary<string, object>
            {
                { "KeyCeremonyId", CurrentKeyCeremony.Id }
            }));
    }

    //[RelayCommand]
    //public async Task KeyCeremonySelectionChanged()
    //{
    //    if (CurrentKeyCeremony == null) return;
    //    await NavigationService.GoToPage(typeof(ViewKeyCeremonyViewModel), new Dictionary<string, object>
    //    {
    //        { "KeyCeremonyId", CurrentKeyCeremony.Id }
    //    });
    //}
}
