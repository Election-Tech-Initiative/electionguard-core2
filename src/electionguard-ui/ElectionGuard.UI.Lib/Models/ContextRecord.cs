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

}
