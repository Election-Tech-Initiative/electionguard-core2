namespace ElectionGuard.ElectionSetup.Records;
/// <summary>
/// Verification of election partial key used in key sharing
/// </summary>
public record ElectionPartialKeyVerificationRecord(string? KeyCeremonyId, string? OwnerId, string? DesignatedId, string? VerifierId, bool Verified);

