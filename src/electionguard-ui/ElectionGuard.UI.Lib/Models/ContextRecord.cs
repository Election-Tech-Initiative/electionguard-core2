namespace ElectionGuard.UI.Lib.Models;

public partial class ContextRecord : DatabaseRecord
{
    [ObservableProperty]
    private string? _electionId;

    [ObservableProperty]
    private string? _contextData;

    public ContextRecord() : base(nameof(ContextRecord))
    {
    }

    public override string ToString() => ContextData ?? string.Empty;
    public static implicit operator string(ContextRecord? record) => record?.ToString() ?? string.Empty;
}
