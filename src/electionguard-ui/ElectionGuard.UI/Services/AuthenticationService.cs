using System.Globalization;
using ElectionGuard.UI.Lib.Services;

namespace ElectionGuard.UI.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        public async Task Login(string username)
        {
            App.CurrentUser.Name = username;
            if (username.ToLower(CultureInfo.CurrentCulture).Contains("admin"))
            {
                App.CurrentUser.IsAdmin = true; // code that is not needed once we have roles in place
                await Shell.Current.GoToAsync($"//{nameof(AdminHomePage)}");
            }
            else
            {
                App.CurrentUser.IsAdmin = false; // code that is not needed once we have roles in place
                await Shell.Current.GoToAsync($"//{nameof(GuardianHomePage)}");
            }
        }

        public string? UserName => App.CurrentUser.Name;
    }
}
