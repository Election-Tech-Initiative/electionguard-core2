using CommunityToolkit.Mvvm.Input;
using ElectionGuard.ElectionSetup;
using ElectionGuard.UI.Lib.Services;

namespace ElectionGuard.UI.ViewModels;

[QueryProperty(CurrentKeyCeremonyParam, "KeyCeremonyId")]
public partial class ViewKeyCeremonyViewModel : BaseViewModel
{
    public const string CurrentKeyCeremonyParam = "KeyCeremonyId";

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
        Task.Run(async() => KeyCeremony = await _keyCeremonyService.GetByIdAsync(value)).Wait();
    }

    [RelayCommand(CanExecute = nameof(CanJoin))]
    public void Join()
    {
        /*
            key_ceremony_id = key_ceremony.id
            user_id = self._auth_service.get_required_user_id()
            self._key_ceremony_service.append_guardian_joined(db, key_ceremony_id, user_id)
            # refresh key ceremony to get the list of guardians with the authoritative order they joined in
            key_ceremony = self._key_ceremony_service.get(db, key_ceremony_id)
            guardian_number = get_guardian_number(key_ceremony, user_id)
            self.log.debug(
                f"user {user_id} about to join key ceremony {key_ceremony_id} as guardian #{guardian_number}"
            )
            guardian = make_guardian(user_id, guardian_number, key_ceremony)
            self._guardian_service.save_guardian(guardian, key_ceremony)
            public_key = guardian.share_key()
            self._key_ceremony_service.append_key(db, key_ceremony_id, public_key)
            self.log.debug(
                f"{user_id} joined key ceremony {key_ceremony_id} as guardian #{guardian_number}"
            )
            self._key_ceremony_service.notify_changed(db, key_ceremony_id)

         */


        var currentGuardianUserName = AuthenticationService.UserName;

        // append guadian joined to key ceremony (db)

        // get guardian number

        // make guardian
        var guardian = Guardian.FromNonce(currentGuardianUserName!, 0, KeyCeremony!.NumberOfGuardians, KeyCeremony.Quorum, KeyCeremony.KeyCeremonyId!);

        // save guardian to local drive / yubikey

        // get public key
        var public_key = guardian.ShareKey();

        // append to key ceremony (db)

        // notify change to admin (signalR)

    }

    private bool CanJoin()
    {
        return KeyCeremony is not null;
    }

    private readonly KeyCeremonyService _keyCeremonyService;
}
