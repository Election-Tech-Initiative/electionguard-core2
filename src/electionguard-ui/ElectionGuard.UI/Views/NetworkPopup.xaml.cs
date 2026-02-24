using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace ElectionGuard.UI.Views;

public partial class NetworkPopup : Popup
{
    public NetworkPopup()
    {
        InitializeComponent();
    }
    private void ReconnectButton_Clicked(object sender, EventArgs e)
    {
        DbService.Reconnect();
        // go to the home page
        (Shell.Current.CurrentPage.BindingContext as BaseViewModel)?.HomeCommand.Execute(null);
        Close();
    }

    private void ExitButton_Clicked(object sender, EventArgs e)
    {
        (Shell.Current.CurrentPage.BindingContext as BaseViewModel)?.LogoutCommand.Execute(null);
        // go to the login page
        Close();
    }
}
