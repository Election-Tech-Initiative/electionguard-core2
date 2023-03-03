namespace ElectionGuard.UI.Lib.Models;

public partial class ConstantsRecord : DatabaseRecord
{
    [ObservableProperty]
    private string? _electionId;

    [ObservableProperty]
    private string? _constantsData;

    public ConstantsRecord() : base(nameof(ConstantsRecord))
    {
    }

}
