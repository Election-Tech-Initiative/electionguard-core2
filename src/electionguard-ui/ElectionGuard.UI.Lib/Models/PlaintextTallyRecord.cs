namespace ElectionGuard.UI.Lib.Models;

public partial class PlaintextTallyRecord : DatabaseRecord
{
    [ObservableProperty]
    private string _tallyId = string.Empty;

    [ObservableProperty]
    private string? _plaintextTallyData;

    public PlaintextTallyRecord() : base(nameof(PlaintextTallyRecord))
    {
    }
}
