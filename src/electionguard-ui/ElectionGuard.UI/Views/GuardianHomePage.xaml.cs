namespace ElectionGuard.UI.Views;

public partial class GuardianHomePage
{
	public GuardianHomePage(GuardianHomeViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
        vm.Page = this;
    }
}