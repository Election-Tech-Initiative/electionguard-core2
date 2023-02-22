using CommunityToolkit.Mvvm.Input;
using ElectionGuard.ElectionSetup;
using ElectionGuard.UI.Helpers;

namespace ElectionGuard.UI.ViewModels;

[QueryProperty(CurrentKeyCeremonyParam, "KeyCeremonyId")]
public partial class ViewKeyCeremonyViewModel : BaseViewModel
{
    public const string CurrentKeyCeremonyParam = "KeyCeremonyId";

    private KeyCeremonyMediator? _mediator;

    private readonly IDispatcherTimer _timer;

    public ViewKeyCeremonyViewModel(IServiceProvider serviceProvider,
                                    KeyCeremonyService keyCeremonyService,
                                    GuardianPublicKeyService guardianService,
                                    GuardianBackupService backupService,
                                    VerificationService verificationService) :
        base("ViewKeyCeremony", serviceProvider)
    {
        _keyCeremonyService = keyCeremonyService;
        _guardianService = guardianService;
        _backupService = backupService;
        _verificationService = verificationService;

        IsJoinVisible = !AuthenticationService.IsAdmin;
        _timer = Dispatcher.GetForCurrentThread()!.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(UISettings.LONG_POLLING_INTERVAL);
        _timer.IsRepeating = true;
        _timer.Tick += CeremonyPollingTimer_Tick;
    }

    [ObservableProperty]
    private KeyCeremony? _keyCeremony;

    [ObservableProperty]
    private bool _isJoinVisible;

    [ObservableProperty]
    private string _keyCeremonyId = string.Empty;

    [ObservableProperty]
    private List<GuardianPublicKey> _guardians = new();

    partial void OnKeyCeremonyIdChanged(string value)
    {
        _ = Task.Run(async () => KeyCeremony = await _keyCeremonyService.GetByKeyCeremonyIdAsync(value));
    }

    private void UpdateKeyCeremony()
    {
        if(KeyCeremonyId != string.Empty)
        {
            OnKeyCeremonyIdChanged(KeyCeremonyId);
        }
    }

    partial void OnKeyCeremonyChanged(KeyCeremony? value)
    {
        if (value is not null)
        {
            _mediator = new KeyCeremonyMediator("mediator", UserName!, value);
            _ = Task.Run(async () => await _mediator.RunKeyCeremony(IsAdmin));

            // load the guardians that have joined
            _ = Task.Run(async () => Guardians = await _guardianService.GetAllByKeyCeremonyIdAsync(value.KeyCeremonyId!));

            IsJoinVisible = !AuthenticationService.IsAdmin && value.State == KeyCeremonyState.PendingGuardiansJoin;

            JoinCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand(CanExecute = nameof(CanJoin))]
    public async Task Join()
    {
        // TODO: Tell the signalR hub what user has joined
        await _mediator!.RunKeyCeremony(IsAdmin);
        _timer.Start();
    }

    private void CeremonyPollingTimer_Tick(object? sender, EventArgs e)
    {
        List<GuardianPublicKey> localData = new();
        _ = Task.Run(async () => await _mediator!.RunKeyCeremony(IsAdmin));
        _ = Task.Run(async () =>
        {
            localData = await _guardianService.GetAllByKeyCeremonyIdAsync(KeyCeremonyId);
            if (localData.Count >= Guardians.Count)
            {
                Guardians = localData;
            }
        });
    }

    private bool CanJoin()
    {
        return KeyCeremony is not null && _mediator is not null;
    }

    private readonly KeyCeremonyService _keyCeremonyService;
    private readonly GuardianPublicKeyService _guardianService;
    private readonly GuardianBackupService _backupService;
    private readonly VerificationService _verificationService;
}
