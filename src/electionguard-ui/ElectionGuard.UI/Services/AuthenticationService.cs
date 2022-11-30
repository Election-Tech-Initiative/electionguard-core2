using ElectionGuard.UI.Lib.Services;
using System.Globalization;

namespace ElectionGuard.UI.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly INavigationService _navigationService;

    public AuthenticationService(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    public async Task Login(string username)
    {
        App.CurrentUser.Name = username;
        var isAdmin = username.ToLower(CultureInfo.CurrentCulture).Contains("admin");
        App.CurrentUser.IsAdmin = isAdmin;
        await _navigationService.GoHome();
    }

    public string? UserName => App.CurrentUser.Name;
}
