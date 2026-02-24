namespace ElectionGuard.UI.Lib.Models;

/// <summary>
/// A list of states for the key ceremony.
/// </summary>
public enum TallyState
{
    DoesNotExist,
    PendingGuardiansJoin,
    TallyStarted,
    AdminAccumulateTally,
    PendingGuardianDecryptShares,
    AdminGenerateChallenge,
    PendingGuardianRespondChallenge,
    AdminVerifyChallenge,
    Complete,
    Abandoned
}
