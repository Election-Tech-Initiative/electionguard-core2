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

    public GuardianBackups(
        string keyCeremonyId,
        string guardianId,
        string designatedId,
        ElectionPartialKeyBackup backup) : base(nameof(GuardianBackups))
    {
        KeyCeremonyId = keyCeremonyId;
        GuardianId = guardianId;
        DesignatedId = designatedId;
        Backup = new(backup);
    }

    public GuardianBackups(GuardianBackups other) : base(nameof(GuardianBackups))
    {
        KeyCeremonyId = other.KeyCeremonyId;
        GuardianId = other.GuardianId;
        DesignatedId = other.DesignatedId;
        Backup = other.Backup != null ? new(other.Backup) : null;
    }

    public void Dispose()
    {
        Backup?.Dispose();
    }
}
