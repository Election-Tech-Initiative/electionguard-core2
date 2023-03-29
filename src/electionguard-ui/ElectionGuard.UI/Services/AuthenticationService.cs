using ElectionGuard.UI.Lib.Services;
using System.Globalization;

namespace ElectionGuard.UI.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly INavigationService _navigationService;
    private readonly UserService _databaseService;

    public AuthenticationService(INavigationService navigationService, UserService userService)
    {
        _navigationService = navigationService;
        _databaseService = userService;
    }

    public async Task Login(string username)
    {
        await Task.Run(() =>
        {
            App.CurrentUser = new();
            App.CurrentUser.Name = username;
            var isAdmin = username.ToLower(CultureInfo.CurrentCulture).Contains("admin");
            App.CurrentUser.IsAdmin = isAdmin;
        });
    }

    public string? UserName => App.CurrentUser.Name;
    public bool IsAdmin => App.CurrentUser.IsAdmin;
}
