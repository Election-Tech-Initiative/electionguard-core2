using CommunityToolkit.Mvvm.ComponentModel;

namespace ElectionGuard.UI.Lib.Models;

public partial class Tally : ObservableObject
{
    [ObservableProperty]
    private string? name;

}

