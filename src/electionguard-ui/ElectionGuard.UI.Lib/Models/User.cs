using CommunityToolkit.Mvvm.ComponentModel;

namespace ElectionGuard.UI.Lib.Models;

public partial class User : ObservableObject
{
    [ObservableProperty]
    private string? name;

    [ObservableProperty]
    private bool isAdmin = false;
}
