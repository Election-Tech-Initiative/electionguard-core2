using ElectionGuard.ElectionSetup.Records;

namespace ElectionGuard.UI.Lib.Models;

/// <summary>
/// Data for the key ceremony that is saved to the database
/// </summary>
public partial class KeyCeremonyRecord : DatabaseRecord, IDisposable
{
    public KeyCeremonyRecord(string name, int numberOfGuardians, int quorum, string admin) : base(nameof(KeyCeremonyRecord))
    {
        Name = name;
        NumberOfGuardians = numberOfGuardians;
        Quorum = quorum;
        KeyCeremonyId = Guid.NewGuid().ToString();
        CreatedAt = DateTime.UtcNow;
        CompletedAt = null;
        State = KeyCeremonyState.PendingGuardiansJoin;
        CreatedBy = admin;
    }

    public KeyCeremonyRecord(KeyCeremonyRecord other) : base(nameof(KeyCeremonyRecord))
    {
        KeyCeremonyId = other.KeyCeremonyId;
        Name = other.Name;
        Quorum = other.Quorum;
        NumberOfGuardians = other.NumberOfGuardians;
        JointKey = other.JointKey != null ? new(other.JointKey) : null;
        CreatedBy = other.CreatedBy;
        CreatedAt = other.CreatedAt;
        CompletedAt = other.CompletedAt;
        UpdatedAt = other.UpdatedAt;
        State = other.State;
    }

    [ObservableProperty]
    private string? _keyCeremonyId;

    [ObservableProperty]
    private string? _name;

    [ObservableProperty]
    private int _quorum;

    [ObservableProperty]
    private int _numberOfGuardians;

    [ObservableProperty]
    private ElectionJointKey? _jointKey;

    [ObservableProperty]
    private string? _createdBy;

    [ObservableProperty]
    private DateTime _createdAt;

    [ObservableProperty]
    private DateTime? _completedAt;

    [ObservableProperty]
    private DateTime? _updatedAt;

    [ObservableProperty]
    private KeyCeremonyState _state;

    public static implicit operator CeremonyDetails(KeyCeremonyRecord data)
    {
        return new(data.KeyCeremonyId!, data.NumberOfGuardians, data.Quorum);
    }

    public void Dispose()
    {
        JointKey?.Dispose();
    }
}
