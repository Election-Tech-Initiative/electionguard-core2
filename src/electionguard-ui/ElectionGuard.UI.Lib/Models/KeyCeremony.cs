namespace ElectionGuard.UI.Lib.Models;

public partial class KeyCeremony : DatabaseRecord
{
    public KeyCeremony(string name, int numberOfGuardians, int quorum)
    {
        _name = name;
        _numberOfGuardians = numberOfGuardians;
        _quorum = quorum;
        KeyCeremonyId = Guid.NewGuid().ToString();
    }

    [ObservableProperty]
    private string? _keyCeremonyId;

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private int _quorum;

    [ObservableProperty]
    private int _numberOfGuardians;
}
