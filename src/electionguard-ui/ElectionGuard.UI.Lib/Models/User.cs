using CommunityToolkit.Mvvm.ComponentModel;

namespace ElectionGuard.UI.Lib.Models;

public partial class User : ObservableObject
{
    [ObservableProperty]
    private string? id;

    [ObservableProperty]
    private string? name;

    [ObservableProperty]
    private bool isAdmin = false;

    [ObservableProperty]
    private string? email = string.Empty;

    [ObservableProperty]
    private string? mobile = string.Empty;
}
