namespace ElectionGuard.UI.Views;

public partial class ElectionPage
{
	public ElectionPage(ElectionViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}