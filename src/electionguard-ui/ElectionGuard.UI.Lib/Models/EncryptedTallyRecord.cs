namespace ElectionGuard.UI.Lib.Models;

public partial class EncryptedTallyRecord : DatabaseRecord
{
    [ObservableProperty]
    private string _tallyId = string.Empty;

    [ObservableProperty]
    private string? _encryptedTallyData;

    public EncryptedTallyRecord() : base(nameof(EncryptedTallyRecord))
    {
    }

    public override string ToString() => EncryptedTallyData ?? string.Empty;
    public static implicit operator string(EncryptedTallyRecord? record) => record?.ToString();

}
