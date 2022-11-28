namespace ElectionGuard.UI.Views;

public partial class AdminHomePage
{
	public AdminHomePage(AdminHomeViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
        vm.Page = this;
    }
}