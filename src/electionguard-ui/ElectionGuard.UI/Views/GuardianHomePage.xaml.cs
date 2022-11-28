namespace ElectionGuard.UI.Views;

public partial class GuardianHomePage : ContentPage
{
	public GuardianHomePage(GuardianHomeViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
        vm.Page = this;
    }
}