namespace ElectionGuard.UI.Models;

public partial class GuardianItem : ObservableObject
{
    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private bool _hasBackup;

    [ObservableProperty]
    private bool _hasVerified;

    [ObservableProperty]
    private bool _badVerified;

    [ObservableProperty]
    private bool _isSelf;
}
