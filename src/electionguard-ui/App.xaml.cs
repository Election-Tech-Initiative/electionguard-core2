namespace ElectionGuard.UI;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new AppShell();
	}

	protected override Window CreateWindow(IActivationState activationState)
	{
		var window = base.CreateWindow(activationState);
		
		if (string.IsNullOrEmpty(window.Title))
		{
			window.Title = GetWindowTitle();
		}

		return window;
	}

	// Hack: get this from a resource
	private string GetWindowTitle() => "ElectionGuard Election Manager";
}
