using CommunityToolkit.Mvvm.DependencyInjection;

namespace ElectionGuard.UI.Views;

public partial class ManifestPopup
{
	public ManifestPopup()
	{
		InitializeComponent();
        var vm = Ioc.Default.GetService(typeof(ManifestViewModel));
		BindingContext = vm;
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        Close();
    }
}
