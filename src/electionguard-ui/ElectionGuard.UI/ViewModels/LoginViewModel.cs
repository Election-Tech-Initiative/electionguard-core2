using CommunityToolkit.Mvvm.Input;

namespace ElectionGuard.UI.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string _name = string.Empty;

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
