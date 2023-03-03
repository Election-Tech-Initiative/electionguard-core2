namespace ElectionGuard.UI.Lib.Models;

public partial class ManifestRecord : DatabaseRecord
{
    [ObservableProperty]
    private string? _electionId;

    [ObservableProperty]
    private string? _manifestData;

    public ManifestRecord() : base(nameof(ManifestRecord))
    {
    }
}
