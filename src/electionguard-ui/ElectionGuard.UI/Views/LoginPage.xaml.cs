namespace ElectionGuard.UI.Views;

public partial class LoginPage
{
    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        vm.Page = this;
    }
}
