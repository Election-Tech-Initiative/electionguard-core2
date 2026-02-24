namespace ElectionGuard.UI.Lib.Models;

public partial class ConstantsRecord : DatabaseRecord
{
    [ObservableProperty]
    private string? _electionId;

    [ObservableProperty]
    private string? _constantsData;

    public ConstantsRecord(string electionId, string constantsData) : base(nameof(ConstantsRecord))
    {
        ElectionId = electionId;
        ConstantsData = constantsData;
    }

    public ConstantsRecord() : base(nameof(ConstantsRecord))
    {
    }

    public override string ToString() => ConstantsData ?? string.Empty;
    public static implicit operator string(ConstantsRecord? record) => record?.ToString() ?? string.Empty;
}
