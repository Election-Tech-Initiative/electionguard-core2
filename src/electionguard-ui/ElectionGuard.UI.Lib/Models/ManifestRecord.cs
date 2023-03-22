namespace ElectionGuard.UI.Lib.Models;

public partial class ManifestRecord : DatabaseRecord
{
    [ObservableProperty]
    private string? _electionId;

    [ObservableProperty]
    private string? _manifestData;

    public ManifestRecord(string electionId, string manifestData) : base(nameof(ManifestRecord))
    {
        ElectionId = electionId;
        ManifestData = manifestData;
    }
    public ManifestRecord() : base(nameof(ManifestRecord))
    {
    }
}
