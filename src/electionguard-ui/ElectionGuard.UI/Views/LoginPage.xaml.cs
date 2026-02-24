namespace ElectionGuard.UI.Views;

public partial class LoginPage
{
    public LoginPage(LoginViewModel vm) : base(vm)
    {
        InitializeComponent();
        Loaded += LoginPage_Loaded;
    }

    private void LoginPage_Loaded(object? sender, EventArgs e)
    {
        var vm = BindingContext as LoginViewModel;
        vm?.OpenSettingsUnsetData();

    }
}
