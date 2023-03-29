namespace ElectionGuard.UI.Lib.Models;

public partial class PlaintextSpoiledBallotRecord : DatabaseRecord
{
    [ObservableProperty]
    private string _tallyId = string.Empty;

    [ObservableProperty]
    private string? _plaintextBallotData;

    public PlaintextSpoiledBallotRecord() : base(nameof(PlaintextSpoiledBallotRecord))
    {
    }
}
