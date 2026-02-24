namespace ElectionGuard.UI.Lib.Models;

public partial class MultiTallyRecord : DatabaseRecord
{
    [ObservableProperty]
    private string? _name;

    [ObservableProperty]
    private string? _multiTallyId;

    [ObservableProperty]
    private string? _keyCeremonyId;

    [ObservableProperty]
    private string _resultsPath = string.Empty;

    [ObservableProperty]
    private List<(string TallyId, string ElectionId, string TallyName)> _tallyIds = new();

    public MultiTallyRecord() : base(nameof(MultiTallyRecord))
    {
        MultiTallyId = Guid.NewGuid().ToString();
    }
}

