namespace ElectionGuard.UI.Views;

public partial class AdminHomePage
{
	public AdminHomePage(AdminHomeViewModel adminHomeViewModel)
	{
		InitializeComponent();
        BindingContext = adminHomeViewModel;
    }
}