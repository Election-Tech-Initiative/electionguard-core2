using CommunityToolkit.Maui.Views;

namespace ElectionGuard.UI.Views;
public partial class SettingsPage : Popup
{
	public SettingsPage()
	{
		InitializeComponent();
		BindingContext = new SettingsViewModel();
	}
}