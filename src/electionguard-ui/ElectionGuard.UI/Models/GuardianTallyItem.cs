namespace ElectionGuard.UI.Models;

public partial class GuardianTallyItem : ObservableObject
{
    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private bool _hasDecryptShares;

    [ObservableProperty]
    private bool _hasResponse;

    [ObservableProperty]
    private bool _isSelf;

    public bool Joined { get; internal set; }
}
