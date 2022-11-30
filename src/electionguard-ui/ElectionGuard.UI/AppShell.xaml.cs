namespace ElectionGuard.UI;

public partial class AppShell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(GuardianHomePage), typeof(GuardianHomePage));
        Routing.RegisterRoute(nameof(AdminHomePage), typeof(AdminHomePage));
        Routing.RegisterRoute(nameof(ElectionPage), typeof(ElectionPage));
    }
}
