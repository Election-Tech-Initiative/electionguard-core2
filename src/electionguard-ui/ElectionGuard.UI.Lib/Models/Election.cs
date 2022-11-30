using CommunityToolkit.Mvvm.ComponentModel;

namespace ElectionGuard.UI.Lib.Models;

public partial class Election : ObservableObject
{
    [ObservableProperty]
    private string? _name;
}
