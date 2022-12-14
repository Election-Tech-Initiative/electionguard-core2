using CommunityToolkit.Mvvm.ComponentModel;

namespace ElectionGuard.UI.Lib.Models;

public partial class Election : BaseModel
{
    [ObservableProperty]
    private string? name;
}
