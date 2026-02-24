using ElectionGuard.ElectionSetup.Records;

namespace ElectionGuard.UI.Lib.Models;

public class GuardianBackups : DatabaseRecord, IDisposable
{
    public string? KeyCeremonyId { get; set; }

    public string? GuardianId { get; set; }

    public string? DesignatedId { get; set; }

    public ElectionPartialKeyBackup? Backup { get; set; }

    public GuardianBackups(GuardianBackups guardianBackups) : base(nameof(GuardianBackups))
    {
        KeyCeremonyId = guardianBackups.KeyCeremonyId;
        GuardianId = guardianBackups.GuardianId;
        DesignatedId = guardianBackups.DesignatedId;
        Backup = guardianBackups.Backup != null ? new ElectionPartialKeyBackup(guardianBackups.Backup) : null;
    }

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

    public GuardianBackups(GuardianBackupsRecord other) : base(nameof(GuardianBackups))
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
