namespace ElectionGuard.ElectionSetup.Records;

public record GuardianBackupsRecord : DisposableRecordBase
{
    public string? KeyCeremonyId { get; set; }

    public string? GuardianId { get; set; }

    public string? DesignatedId { get; set; }

    public ElectionPartialKeyBackupRecord? Backup { get; set; }

    public GuardianBackupsRecord(
        string keyCeremonyId,
        string guardianId,
        string designatedId,
        ElectionPartialKeyBackupRecord backup) 
    {
        KeyCeremonyId = keyCeremonyId;
        GuardianId = guardianId;
        DesignatedId = designatedId;
        Backup = new(backup.OwnerId, backup.OwnerSequenceOrder, backup.DesignatedId, backup.DesignatedSequenceOrder, backup.EncryptedCoordinate);
    }
    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();
        Backup?.Dispose();
    }

}
