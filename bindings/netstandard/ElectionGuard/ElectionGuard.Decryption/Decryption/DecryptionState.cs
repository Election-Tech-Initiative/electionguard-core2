namespace ElectionGuard.Decryption.Decryption;

public enum DecryptionState
{
    DoesNotExist = 0,
    PendingGuardianShares = 1,
    PendingAdminChallenge = 2,
    PendingGuardianChallengeResponses = 3,
    PendingAdminValidateResponses = 4, // maybe should be pending proof generqation?
    PendingAdminDecryptShares = 5,
    PendingAdminPublishResults = 6,
    Complete = 7
}
