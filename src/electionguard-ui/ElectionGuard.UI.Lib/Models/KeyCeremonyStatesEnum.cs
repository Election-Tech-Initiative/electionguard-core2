namespace ElectionGuard.UI.Lib.Models;

/// <summary>
/// A list of states for the key ceremony.
/// </summary>
public enum KeyCeremonyState
{
    DoesNotExist = 0,
    PendingGuardiansJoin = 1,
    PendingAdminAnnounce = 2,
    PendingGuardianBackups = 3,
    PendingAdminToShareBackups = 4,
    PendingGuardiansVerifyBackups = 5,
    PendingAdminToPublishJointKey = 6,
    Complete = 7
}
