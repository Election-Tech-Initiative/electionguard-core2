using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace ElectionGuard.UI.Lib.Models;

public partial class User : BaseModel
{
    [ObservableProperty]
    private string? _name;

    [ObservableProperty]
    private bool _isAdmin = false;

    [ObservableProperty]
    private string? _email = string.Empty;

    [ObservableProperty]
    private string? _mobile = string.Empty;

    static User()
    {
        table = "user";
    }
}
