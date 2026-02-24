namespace ElectionGuard.UI.Lib.Models;

public partial class CiphertextTallyRecord : DatabaseRecord
{
    [ObservableProperty]
    private string _tallyId = string.Empty;

    [ObservableProperty]
    private string _electionId = string.Empty;

    [ObservableProperty]
    private string _uploadId = string.Empty;

    [ObservableProperty]
    private bool _isExportable = false;

    [ObservableProperty]
    private string _ciphertextTallyData = string.Empty;

    public CiphertextTallyRecord() : base(nameof(CiphertextTallyRecord))
    {
    }

    public override string ToString() => CiphertextTallyData ?? string.Empty;
    public static implicit operator string(CiphertextTallyRecord? record) => record?.ToString() ?? string.Empty;

}
