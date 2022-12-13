namespace ElectionGuard.UI.Lib.Models;

public partial class KeyCeremony : BaseModel<KeyCeremony>
{
    private readonly static string table = "key_ceremonies";

    public KeyCeremony(string name, int numberOfGuardians, int quorum) : base(table)
    {
        _name = name;
        _numberOfGuardians = numberOfGuardians;
        _quorum = quorum;
    }

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private int _quorum;

    [ObservableProperty]
    private int _numberOfGuardians;
}
