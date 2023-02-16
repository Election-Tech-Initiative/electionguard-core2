using CommunityToolkit.Mvvm.Input;
using ElectionGuard.ElectionSetup;
using ElectionGuard.UI.Helpers;

namespace ElectionGuard.UI.ViewModels;

[QueryProperty(CurrentKeyCeremonyParam, "KeyCeremonyId")]
public partial class ViewKeyCeremonyViewModel : BaseViewModel
{
    public const string CurrentKeyCeremonyParam = "KeyCeremonyId";

    private KeyCeremonyMediator? _mediator;

    private bool joined = false;

    public ViewKeyCeremonyViewModel(IServiceProvider serviceProvider, KeyCeremonyService keyCeremonyService) : 
        base("ViewKeyCeremony", serviceProvider)
    {
        _keyCeremonyService = keyCeremonyService;
        IsJoinVisible = !AuthenticationService.IsAdmin;
    }

    [ObservableProperty]
    private KeyCeremony? _keyCeremony;

    [ObservableProperty]
    private bool _isJoinVisible;

    [ObservableProperty]
    private string _keyCeremonyId = string.Empty;

    partial void OnKeyCeremonyIdChanged(string value)
    {
        Task.Run(async() => KeyCeremony = await _keyCeremonyService.GetByKeyCeremonyIdAsync(value));
    }

    partial void OnKeyCeremonyChanged(KeyCeremony? value)
    {
        if (value is not null)
        {
            _mediator = new KeyCeremonyMediator("mediator", UserName!, value);
            Task.Run(async () => await _mediator.RunKeyCeremony(IsAdmin));
        }
    }

    [RelayCommand(CanExecute = nameof(CanJoin))]
    public async Task Join()
    {
        joined = true;
        // TODO: Tell the signalR hub what user has joined
        await _mediator!.RunKeyCeremony(IsAdmin);
        var timer = Dispatcher.GetForCurrentThread()!.CreateTimer();
        timer.Interval = TimeSpan.FromSeconds(UISettings.LONG_POLLING_INTERVAL);
        timer.IsRepeating = true;
        timer.Tick += CeremonyPollingTimer_Tick;
    }

    private void CeremonyPollingTimer_Tick(object? sender, EventArgs e)
    {
        Task.Run(async () => await _mediator!.RunKeyCeremony(IsAdmin));
    }

    private bool CanJoin()
    {
        return KeyCeremony is not null && _mediator is not null;
    }

    private readonly KeyCeremonyService _keyCeremonyService;
}
