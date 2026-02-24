using ElectionGuard.ElectionSetup.Records;

namespace ElectionGuard.ElectionSetup;

/// <summary>
/// The state of the verifications of all guardian election partial key backups
/// </summary>
public record BackupVerificationState(bool AllSent = false, bool AllVerified = false, List<GuardianPair>? FailedVerification = null);

