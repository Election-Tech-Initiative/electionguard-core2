namespace ElectionGuard.UI.Lib.Models;

public partial class TallyRecord : DatabaseRecord
{
    [ObservableProperty]
    private string? _name;

    [ObservableProperty]
    private string? _electionId;

    [ObservableProperty]
    private string? _keyCeremonyId;

    [ObservableProperty]
    private string _tallyId = string.Empty;

    [ObservableProperty]
    private int _quorum;

    [ObservableProperty]
    private int _numberOfGuardians;

    [ObservableProperty]
    private long _ballotCount;

    [ObservableProperty]
    private long _ballotUploadCount;

    [ObservableProperty]
    private TallyState _state;

    [ObservableProperty]
    private DateTime? _lastExport = null;

    [ObservableProperty]
    private List<long> _deviceIds = new();

    [ObservableProperty]
    private List<long> _dates = new();

    [ObservableProperty]
    private DateTime _createdAt;

    public TallyRecord() : base(nameof(TallyRecord))
    {
        TallyId = Guid.NewGuid().ToString();
        CreatedAt = DateTime.UtcNow;
    }
}

