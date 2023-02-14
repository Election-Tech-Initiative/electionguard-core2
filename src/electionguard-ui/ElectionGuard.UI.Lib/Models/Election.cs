using CommunityToolkit.Mvvm.ComponentModel;

namespace ElectionGuard.UI.Lib.Models;

public partial class Election : DatabaseRecord
{
    [ObservableProperty]
    private string? _electionId;

    [ObservableProperty]
    private string? _name;

    public Election() : base(nameof(Election))
    {

    }

}
