namespace ElectionGuard.Decryption.Decryption;

public enum DecryptionState
{
    DoesNotExist = 0,
    PendingGuardians = 1,
    PendingAdminAnnounce = 2,
    PendingGuardianBackups = 3,
    PendingAdminToShareBackups = 4,
    PendingGuardiansVerifyBackups = 5,
    PendingAdminToPublishJointKey = 6,
    Complete = 7
}
