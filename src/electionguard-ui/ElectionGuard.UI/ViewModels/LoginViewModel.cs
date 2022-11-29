using CommunityToolkit.Mvvm.Input;
using ElectionGuard.UI.Lib.Services;
using ElectionGuard.UI.Views;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectionGuard.UI.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string name = string.Empty;

        [RelayCommand(CanExecute = nameof(CanLogin))]
        async Task Login()
        {
            App.CurrentUser.Name = Name;
            if (Name.ToLower().Contains("admin"))
            {
                App.CurrentUser.IsAdmin = true; // code that is not needed once we have roles in place
                App.CurrentUser.Save();
                await Shell.Current.GoToAsync($"//{nameof(AdminHomePage)}");
            }
            else
            {
                App.CurrentUser.IsAdmin = false; // code that is not needed once we have roles in place
                App.CurrentUser.Save();
                await Shell.Current.GoToAsync($"//{nameof(GuardianHomePage)}");
            }
            Name = string.Empty;                // reset the UI name field
        }

        bool CanLogin()
        {
            return Name.Trim().Length > 0;
        }
    }
}
