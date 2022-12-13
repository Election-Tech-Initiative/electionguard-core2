using CommunityToolkit.Mvvm.ComponentModel;

namespace ElectionGuard.UI.Lib.Models;

public partial class User : BaseModel<User>
{
    private readonly static string table = "users";

    public User() : base(table)
    {
    }

    [ObservableProperty]
    private string? name;

    [ObservableProperty]
    private bool isAdmin = false;

}
