namespace ElectionGuard.UI.Lib.Models;

/// <summary>
/// Verification of election partial key used in key sharing
/// </summary>
public class ElectionPartialKeyVerification : DatabaseRecord
{
    public string? KeyCeremonyId { get; set; }
    public string? OwnerId { get; init; }
    public string? DesignatedId { get; init; }
    public string? VerifierId { get; init; }
    public bool Verified { get; init; }

    public ElectionPartialKeyVerification() : base(nameof(ElectionPartialKeyVerification))
    {

    }

    public ElectionPartialKeyVerification(ElectionPartialKeyVerification other) : base(nameof(ElectionPartialKeyVerification))
    {
        KeyCeremonyId = other.KeyCeremonyId;
        OwnerId = other.OwnerId;
        DesignatedId = other.DesignatedId;
        VerifierId = other.VerifierId;
        Verified = other.Verified;
    }
}
