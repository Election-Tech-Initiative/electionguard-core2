namespace ElectionGuard.UI.Lib.Models;

/// <summary>
/// A list of states for the key ceremony.
/// </summary>
public enum TallyState
{
    DoesNotExist = 0,
    PendingGuardiansJoin = 1,
    PendingAdminToTallyVerify = 2,
    Complete = 3
}
