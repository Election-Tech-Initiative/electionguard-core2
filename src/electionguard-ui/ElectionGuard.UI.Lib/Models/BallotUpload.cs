namespace ElectionGuard.UI.Lib.Models;

public partial class BallotUpload : DatabaseRecord
{
    [ObservableProperty]
    private string? _electionId;

    [ObservableProperty]
    private string? _uploadId;

    [ObservableProperty]
    private string? _deviceFileName;

    [ObservableProperty]
    private string? _deviceFileContents;

    [ObservableProperty]
    private string? _location;

    [ObservableProperty]
    private long _launchCode;

    [ObservableProperty]
    private long _deviceId;

    [ObservableProperty]
    private long _sessionId;

    [ObservableProperty]
    private long _ballotCount;

    [ObservableProperty]
    private long _ballotImported;

    [ObservableProperty]
    private long _ballotChallenged;

    [ObservableProperty]
    private long _ballotSpoiled;

    [ObservableProperty]
    private long _ballotDuplicated;

    [ObservableProperty]
    private long _ballotRejected;

    [ObservableProperty]
    private long _serialNumber;

    [ObservableProperty]
    private string? _createdBy;

    [ObservableProperty]
    private DateTime _createdAt;

    [ObservableProperty]
    private DateTime _ballotsStart;

    [ObservableProperty]
    private DateTime _ballotsEnd;

    public BallotUpload() : base(nameof(BallotUpload))
    {
        UploadId = Guid.NewGuid().ToString();
        CreatedAt = DateTime.UtcNow;
    }
}
