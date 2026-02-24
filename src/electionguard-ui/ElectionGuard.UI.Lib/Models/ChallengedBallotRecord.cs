namespace ElectionGuard.UI.Lib.Models;

public partial class ChallengedBallotRecord : DatabaseRecord
{
    [ObservableProperty]
    private string _electionId = string.Empty;

    [ObservableProperty]
    private string _tallyId = string.Empty;

    [ObservableProperty]
    private string _ballotCode = string.Empty;

    [ObservableProperty]
    private string _ballotData = string.Empty;

    public ChallengedBallotRecord() : base(nameof(ChallengedBallotRecord))
    {
    }

    public override string ToString() => BallotData ?? string.Empty;
    public static implicit operator string(ChallengedBallotRecord? record) => record?.ToString() ?? string.Empty;
}
