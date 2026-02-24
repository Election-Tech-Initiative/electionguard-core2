namespace ElectionGuard.UI.Lib.Models;

public partial class TallyItem : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _party = string.Empty;

    [ObservableProperty]
    private ulong _votes;

    [ObservableProperty]
    private float _percent;

}
