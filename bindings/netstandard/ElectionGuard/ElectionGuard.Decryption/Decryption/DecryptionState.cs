namespace ElectionGuard.Decryption.Decryption;

// TODO: use in state machine as part of proof generation and verification.
public enum DecryptionState
{
    DoesNotExist = 0,
    PendingGuardianShares = 1,
    PendingGuardianCommitments = 2,
    PendingAdminCommitmentChallenge = 3,
    PendingGuardianChallengeResponse = 4,
    PendingAdminVerifyResults = 5,
    PendingAdminPublishResults = 6,
    Complete = 7
}
