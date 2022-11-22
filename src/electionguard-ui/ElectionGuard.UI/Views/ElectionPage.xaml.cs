namespace ElectionGuard.UI.Views;

public partial class ElectionPage : ContentPage
{
	public ElectionPage(ElectionViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
		vm.Page = this;
	}
}