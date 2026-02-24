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
    private long _castBallotCount;

    [ObservableProperty]
    private long _challengedBallotCount;

    [ObservableProperty]
    private long _spoiledBallotCount;

    [ObservableProperty]
    private TallyState _state;

    [ObservableProperty]
    private bool _multiTally;

    [ObservableProperty]
    private bool _viewed;

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

