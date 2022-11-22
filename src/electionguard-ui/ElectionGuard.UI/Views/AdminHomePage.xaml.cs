namespace ElectionGuard.UI.Views;

public partial class AdminHomePage : ContentPage
{
	public AdminHomePage(AdminHomeViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
        vm.Page = this;
    }
}