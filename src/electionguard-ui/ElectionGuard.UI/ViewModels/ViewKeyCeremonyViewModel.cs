using CommunityToolkit.Mvvm.Input;
using ElectionGuard.ElectionSetup;
using ElectionGuard.UI.Models;

namespace ElectionGuard.UI.ViewModels;


[QueryProperty(CurrentKeyCeremonyParam, "KeyCeremonyId")]
public partial class ViewKeyCeremonyViewModel : BaseViewModel
{
    public const string CurrentKeyCeremonyParam = "KeyCeremonyId";

    private KeyCeremonyMediator? _mediator;

    private readonly IDispatcherTimer _timer;

    private bool joined;

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

    [ObservableProperty]
    private ObservableCollection<GuardianItem> _guardianList = new();

    public override async Task OnLeavingPage()
    {
        _timer.Stop();
        await Task.Yield();
    }


    partial void OnKeyCeremonyIdChanged(string value)
    {
        _ = Task.Run(async () => KeyCeremony = await _keyCeremonyService.GetByKeyCeremonyIdAsync(value));
    }

    private void UpdateKeyCeremony()
    {
        if (KeyCeremonyId != string.Empty)
        {
            OnKeyCeremonyIdChanged(KeyCeremonyId);
        }
    }

    partial void OnKeyCeremonyChanged(KeyCeremony? value)
    {
        if (value is not null)
        {
            IsJoinVisible = (!AuthenticationService.IsAdmin && (value.State == KeyCeremonyState.PendingGuardiansJoin));

            _mediator = new KeyCeremonyMediator("mediator", UserName!, value);

            if (IsJoinVisible is false)
            {
                _timer.Start();
                CeremonyPollingTimer_Tick(this, null);
            }

            // load the guardians that have joined
            //            _ = Task.Run(UpdateGuardiansData);

            JoinCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand(CanExecute = nameof(CanJoin))]
    public async Task Join()
    {
        joined = true;
        // TODO: Tell the signalR hub what user has joined
        await _mediator!.RunKeyCeremony(IsAdmin);
        _timer.Start();
    }

    private void CeremonyPollingTimer_Tick(object? sender, EventArgs e)
    {
        if (KeyCeremony.State == KeyCeremonyState.Complete)
        {
            _timer.Stop();
            return;
        }
        List<GuardianPublicKey> localData = new();
        _ = Task.Run(async () => await _mediator!.RunKeyCeremony(IsAdmin));
        _ = Task.Run(UpdateGuardiansData);
    }

    private async Task UpdateGuardiansData()
    {
        var localData = await _guardianService.GetAllByKeyCeremonyIdAsync(KeyCeremonyId);

        foreach (var item in localData)
        {
            if (GuardianList.Count(i => i.Name == item.GuardianId) == 0)
            {
                GuardianList.Add(new GuardianItem() { Name = item.GuardianId });
            }
        }
        foreach (var guardian in GuardianList)
        {
            var key = new GuardianPair(guardian.Name, guardian.Name);
            guardian.HasBackup = await _mediator!.HasBackup(guardian.Name);
            (guardian.HasVerified, guardian.BadVerified) = await _mediator!.HasVerified(guardian.Name);
            guardian.IsSelf = guardian.Name == UserName!;
        }
    }


    private bool CanJoin()
    {
        return KeyCeremony is not null && _mediator is not null && joined is false;
    }

    private readonly KeyCeremonyService _keyCeremonyService;
    private readonly GuardianPublicKeyService _guardianService;
    private readonly GuardianBackupService _backupService;
    private readonly VerificationService _verificationService;
}
