using CommunityToolkit.Mvvm.ComponentModel;

namespace ElectionGuard.UI.Lib.Models;

public partial class Tally : BaseModel
{
    [ObservableProperty]
    private string? _name;

}

