namespace ElectionGuard.UI.Lib.Models;

public class GuardianPublicKey : DatabaseRecord
{
    public string? KeyCeremonyId { get; set; }

    public string? GuardianId { get; set; }

    public ElectionPublicKey? PublicKey { get; set; }

    public GuardianPublicKey() : base(nameof(GuardianPublicKey))
    {
    }
}
