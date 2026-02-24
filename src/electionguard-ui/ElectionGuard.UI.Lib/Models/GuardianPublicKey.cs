using ElectionGuard.Guardians;

namespace ElectionGuard.UI.Lib.Models;


public class GuardianPublicKey : DatabaseRecord, IDisposable
{
    // TODO: implement IElectionGuardian by including the sequence order

    public string? KeyCeremonyId { get; set; }

    public string? GuardianId { get; set; }

    public ElectionPublicKey? PublicKey { get; set; }

    public GuardianPublicKey() : base(nameof(GuardianPublicKey))
    {
    }

    public GuardianPublicKey(GuardianPublicKey other) : base(nameof(GuardianPublicKey))
    {
        KeyCeremonyId = other.KeyCeremonyId;
        GuardianId = other.GuardianId;
        PublicKey = other.PublicKey != null ? new(other.PublicKey) : null;
    }

    public void Dispose()
    {
        PublicKey?.Dispose();
    }
}
