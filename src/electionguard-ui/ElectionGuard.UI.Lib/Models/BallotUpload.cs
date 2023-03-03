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
    private long _ballotCount;

    [ObservableProperty]
    private string? _createdBy;

    [ObservableProperty]
    private DateTime _createdAt;

    public BallotUpload() : base(nameof(BallotUpload))
    {

    }

}
