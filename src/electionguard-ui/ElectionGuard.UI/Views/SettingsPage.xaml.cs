namespace ElectionGuard.UI.Views;

public partial class SettingsPage
{
	public SettingsPage()
	{
		InitializeComponent();
		BindingContext = new SettingsViewModel();
	}
}