using Windows.UI.Core;

namespace ElectionGuard.UI.Views;

public partial class LoginPage
{
	public LoginPage(LoginViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
		vm.Page = this;
        CoreWindow.GetForCurrentThread().KeyDown += LoginPage_KeyDown;
    }

    private void LoginPage_KeyDown(CoreWindow sender, KeyEventArgs args)
    {
        if (args.VirtualKey == Windows.System.VirtualKey.Enter)
        {
            LoginButton.SendClicked();
        }
    }
}