using CommunityToolkit.Mvvm.ComponentModel;

namespace ElectionGuard.UI.Lib.Models;

public partial class Election : BaseModel<Tally>
{
    private readonly static string table = "elections";

    public Election() : base(table)
    {
    }

    [ObservableProperty]
    private string? name;
}
