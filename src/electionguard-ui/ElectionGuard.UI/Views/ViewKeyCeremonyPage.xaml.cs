namespace ElectionGuard.UI.Views;

public partial class ViewKeyCeremonyPage
{
	public ViewKeyCeremonyPage(ViewKeyCeremonyViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}