namespace ElectionGuard.UI.Lib.Models;

public partial class KeyCeremony : ObservableObject
{
    public KeyCeremony(string name, int numberOfGuardians, int quorum)
    {
        _name = name;
        _numberOfGuardians = numberOfGuardians;
        _quorum = quorum;
    }

    [ObservableProperty] 
    private int _id;

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private int _quorum;

    [ObservableProperty]
    private int _numberOfGuardians;
}
