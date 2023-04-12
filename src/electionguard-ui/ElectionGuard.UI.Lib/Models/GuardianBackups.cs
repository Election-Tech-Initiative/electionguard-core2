namespace ElectionGuard.UI.Lib.Models;

public class GuardianBackups : DatabaseRecord, IDisposable
{
    public string? KeyCeremonyId { get; set; }

    public string? GuardianId { get; set; }

    public string? DesignatedId { get; set; }

    public ElectionPartialKeyBackup? Backup { get; set; }

    public GuardianBackups() : base(nameof(GuardianBackups))
    {
    }

    public void Dispose()
    {
        Backup?.Dispose();
    }
}
