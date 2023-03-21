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
    private long _ballotCount;

    [ObservableProperty]
    private long _ballotImported;

    [ObservableProperty]
    private long _ballotSpoiled;

    [ObservableProperty]
    private long _ballotRejected;

    [ObservableProperty]
    private uint _serialNumber;

    [ObservableProperty]
    private string? _createdBy;

    [ObservableProperty]
    private DateTime _createdAt;

    public BallotUpload(string electionId, string deviceFilename, string deviceFileContents, string location, long ballotCount, long ballotImported, long ballotSpoiled, long ballotRejected, uint serialNumber, string createdBy) : base(nameof(BallotUpload))
    {
        ElectionId = electionId;
        DeviceFileName = deviceFilename;
        DeviceFileContents = deviceFileContents;
        Location = location;
        BallotCount = ballotCount;
        BallotImported = ballotImported;
        BallotSpoiled = ballotSpoiled;
        BallotRejected = ballotRejected;
        SerialNumber = serialNumber;
        CreatedBy = createdBy;
        UploadId = Guid.NewGuid().ToString();
        CreatedAt = DateTime.UtcNow;
    }

}
