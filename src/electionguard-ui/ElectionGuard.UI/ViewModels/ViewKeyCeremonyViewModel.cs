using CommunityToolkit.Mvvm.Input;
using ElectionGuard.UI.Models;

namespace ElectionGuard.UI.ViewModels;

[QueryProperty(CurrentKeyCeremonyParam, "KeyCeremony")]
public partial class ViewKeyCeremonyViewModel : BaseViewModel
{
    public const string CurrentKeyCeremonyParam = "KeyCeremony";

    private KeyCeremonyMediator? _mediator;

    private bool _joinPressed;

    public ViewKeyCeremonyViewModel(IServiceProvider serviceProvider,
                                    KeyCeremonyService keyCeremonyService,
                                    GuardianPublicKeyService guardianService,
                                    GuardianBackupService backupService,
                                    VerificationService verificationService,
                                    ILogger<ViewKeyCeremonyViewModel> logger) :
        base("ViewKeyCeremony", serviceProvider)
    {
        _logger = logger;
        _keyCeremonyService = keyCeremonyService;
        _publicKeyService = guardianService;
        _backupService = backupService;
        _verificationService = verificationService;

        IsJoinVisible = !AuthenticationService.IsAdmin;
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(JoinCommand))]
    private KeyCeremonyRecord? _keyCeremony;

    [ObservableProperty]
    private bool _isJoinVisible;

    [ObservableProperty]
    private string _keyCeremonyId = string.Empty;

    [ObservableProperty]
    private ObservableCollection<GuardianItem> _guardianList = new();

    public override async Task OnAppearing()
    {
        await base.OnAppearing();

        _timer!.Tick += CeremonyPollingTimer_Tick;

        try
        {
            _joinPressed = await HasJoined();
            if (!_timer.IsRunning)
            {
                _timer.Start();
            }
        }
        catch (Exception)
        {
            _timer.Stop();
        }
    }

    public override async Task OnLeavingPage()
    {
        await base.OnLeavingPage();
        _timer!.Tick -= CeremonyPollingTimer_Tick;
        _timer.Stop();

        KeyCeremony?.Dispose();
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
            KeyCeremonyId = value.KeyCeremonyId!;
            IsJoinVisible = (!AuthenticationService.IsAdmin && (value.State == KeyCeremonyState.PendingGuardiansJoin));

            _mediator = new KeyCeremonyMediator(
                "mediator",
                UserName!,
                value,
                _keyCeremonyService,
                _backupService,
                _publicKeyService,
                _verificationService);

            if (!IsJoinVisible)
            {
                _timer!.Start();
                CeremonyPollingTimer_Tick(this, null);
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanJoin))]
    public async Task Join()
    {
        _joinPressed = true;
        // TODO: Tell the signalR hub what user has joined
        try
        {
            await _mediator!.RunKeyCeremony(IsAdmin);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _logger.LogError(ex, "Exception in Key Ceremony at {KeyCeremony.State}", KeyCeremony.State);
        }
    }

    private void CeremonyPollingTimer_Tick(object? sender, EventArgs e)
    {
        if (KeyCeremony is null)
        {
            return;
        }

        if (KeyCeremony.State == KeyCeremonyState.Complete)
        {
            _timer!.Stop();
            return;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                await UpdateGuardiansData();
                if (IsAdmin || (!IsAdmin && _joinPressed))
                {
                    await _mediator!.RunKeyCeremony(IsAdmin);
                    ErrorMessage = string.Empty;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                _logger.LogError(ex, "Exception in Key Ceremony at {KeyCeremony.State}", KeyCeremony.State);
            }
        });
    }

    private async Task<bool> HasJoined()
    {
        ElectionGuardException.ThrowIfNull(UserName);

        var publicKey = await _publicKeyService.GetByIdsAsync(KeyCeremonyId, UserName);

        return publicKey is not null;
    }

    private async Task UpdateGuardiansData()
    {
        // if we have fewer than max number, see if anyone else joined
        if (GuardianList.Count != KeyCeremony.NumberOfGuardians)
        {
            var localData = await _publicKeyService.GetAllByKeyCeremonyIdAsync(KeyCeremonyId);

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
    private readonly GuardianPublicKeyService _publicKeyService;
    private readonly GuardianBackupService _backupService;
    private readonly VerificationService _verificationService;
}
