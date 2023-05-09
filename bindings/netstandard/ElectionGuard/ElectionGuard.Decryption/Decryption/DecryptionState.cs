namespace ElectionGuard.Decryption.Decryption;

public enum DecryptionState
{
    DoesNotExist = 0,
    PendingGuardianShares = 1,
    PendingAdminChallenge = 2,
    PendingGuardianChallengeResponses = 3,
    PendingAdminValidateResponses = 4,
    PendingAdminComputeProofs = 5,
    PendingAdminDecryptShares = 6,
    PendingAdminPublishResults = 7,
    Complete = 8
}
