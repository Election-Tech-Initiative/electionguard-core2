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

    public override string ToString() => PlaintextTallyData ?? string.Empty;
    public static implicit operator string(PlaintextTallyRecord? record) => record?.ToString() ?? string.Empty;

}
