using System.Text.Json.Serialization;

namespace ElectionGuard.UI.Lib.Models;

/// <summary>
/// The Election joint key
/// </summary>
public class ElectionJointKey : DisposableBase
{
    /// <summary>
    /// The product of the guardian public keys
    /// K = ∏ ni=1 Ki mod p.
    /// </summary>
    public ElementModP? JointPublicKey { get; init; }

    /// <summary>
    /// The hash of the commitments that the guardians make to each other
    /// H = H(K 1,0 , K 2,0 ... , K n,0 )
    /// </summary>
    public ElementModQ? CommitmentHash { get; init; }

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();

        JointPublicKey?.Dispose();
        CommitmentHash?.Dispose();
    }
}

public class GuardianPublicKey : DatabaseRecord
{
    public string? KeyCeremonyId { get; set; }

    public string? GuardianId { get; set; }

    public ElectionPublicKey? PublicKey { get; set; }

    public GuardianPublicKey() : base(nameof(GuardianPublicKey))
    {
    }
}

public class GuardianBackups : DatabaseRecord
{
    public string? KeyCeremonyId { get; set; }

    public string? GuardianId { get; set; }

    public string? DesignatedId { get; set; }

    public ElectionPartialKeyBackup? Backup { get; set; }

    public GuardianBackups() : base(nameof(GuardianBackups))
    {
    }
}



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


//"guardians_joined": [],
//"keys": [],
//"guardians_keys": [],
//"other_keys": [],
//"backups": [],
//"shared_backups": [],
//"verifications": [],


