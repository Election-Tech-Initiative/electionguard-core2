using CommunityToolkit.Mvvm.Input;
using ElectionGuard.ElectionSetup;
using ElectionGuard.UI.Models;

namespace ElectionGuard.UI.ViewModels;

[QueryProperty(CurrentKeyCeremonyParam, "KeyCeremonyId")]
public partial class ViewKeyCeremonyViewModel : BaseViewModel
{
    public const string CurrentKeyCeremonyParam = "KeyCeremonyId";

    private KeyCeremonyMediator? _mediator;

    private bool _joinPressed;

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
    }

    [ObservableProperty]
    private KeyCeremonyRecord? _keyCeremony;

    [ObservableProperty]
    private bool _isJoinVisible;

    [ObservableProperty]
    private string _keyCeremonyId = string.Empty;

    [ObservableProperty]
    private List<GuardianPublicKey> _guardians = new();

    [ObservableProperty]
    private ObservableCollection<GuardianItem> _guardianList = new();

    public override async Task OnAppearing()
    {
        await base.OnAppearing();

        _timer.Tick += CeremonyPollingTimer_Tick;

        if (!_timer.IsRunning)
        {
            _timer.Start();
        }
    }

    public override async Task OnLeavingPage()
    {
        await base.OnLeavingPage();
        _timer.Tick -= CeremonyPollingTimer_Tick;
        _timer.Stop();
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

    partial void OnKeyCeremonyChanged(KeyCeremonyRecord? value)
    {
        if (value is not null)
        {
            IsJoinVisible = (!AuthenticationService.IsAdmin && (value.State == KeyCeremonyState.PendingGuardiansJoin));

            _mediator = new KeyCeremonyMediator("mediator", UserName!, value);

            if (!IsJoinVisible)
            {
                _timer.Start();
                CeremonyPollingTimer_Tick(this, null);
            }

            JoinCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand(CanExecute = nameof(CanJoin))]
    public async Task Join()
    {
        _joinPressed = true;
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
        _ = Task.Run(async () =>
        {
            await _mediator!.RunKeyCeremony(IsAdmin);
            await UpdateGuardiansData();
        });
    }

    private async Task UpdateGuardiansData()
    {
        // if we have fewer than max number, see if anyone else joined
        if (GuardianList.Count != KeyCeremony.NumberOfGuardians)
        {
            var localData = await _guardianService.GetAllByKeyCeremonyIdAsync(KeyCeremonyId);

            foreach (var item in localData)
            {
                if (!GuardianList.Any(g => g.Name == item.GuardianId))
                {
                    GuardianList.Add(new GuardianItem() { Name = item.GuardianId });
                }
            }
        }
        foreach (var guardian in GuardianList)
        {
            guardian.HasBackup = await _mediator!.HasBackup(guardian.Name);
            (guardian.HasVerified, guardian.BadVerified) = await _mediator!.HasVerified(guardian.Name);
            guardian.IsSelf = guardian.Name == UserName!;
        }
    }

    private bool CanJoin()
    {
        return KeyCeremony is not null && _mediator is not null && _joinPressed is false;
    }

    private readonly KeyCeremonyService _keyCeremonyService;
    private readonly GuardianPublicKeyService _guardianService;
    private readonly GuardianBackupService _backupService;
    private readonly VerificationService _verificationService;
}
