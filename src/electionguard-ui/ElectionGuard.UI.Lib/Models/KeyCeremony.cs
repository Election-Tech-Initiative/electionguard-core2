using System.Text.Json.Serialization;

namespace ElectionGuard.UI.Lib.Models;

/// <summary>
/// Data for the key ceremony that is saved to the database
/// </summary>
public partial class KeyCeremony : DatabaseRecord
{
    public KeyCeremony(string name, int numberOfGuardians, int quorum, string admin) : base(nameof(KeyCeremony))
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
    private KeyCeremonyState _state;
}
