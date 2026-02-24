namespace ElectionGuard.UI.Views;

public partial class SettingsPage
{
    public SettingsPage(SettingsViewModel settingsViewModel)
    {
        InitializeComponent();
        BindingContext = settingsViewModel;
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        Close();
    }
}