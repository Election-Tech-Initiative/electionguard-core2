namespace ElectionGuard.UI.Lib.Models;

public partial class ChallengeRecord : DatabaseRecord
{
    [ObservableProperty]
    private string _tallyId = string.Empty;

    [ObservableProperty]
    private string _guardianId = string.Empty;

    [ObservableProperty]
    private string _challengeData = string.Empty;

    public ChallengeRecord() : base(nameof(ChallengeRecord))
    {
    }

    public override string ToString() => ChallengeData ?? string.Empty;
    public static implicit operator string(ChallengeRecord? record) => record?.ToString() ?? string.Empty;
}
