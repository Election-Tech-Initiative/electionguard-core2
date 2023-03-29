namespace ElectionGuard.UI.Lib.Models;

public partial class CiphertextTallyRecord : DatabaseRecord
{
    [ObservableProperty]
    private string _tallyId = string.Empty;

    [ObservableProperty]
    private string? _ciphertextTallyData;

    public CiphertextTallyRecord() : base(nameof(CiphertextTallyRecord))
    {
    }
}
