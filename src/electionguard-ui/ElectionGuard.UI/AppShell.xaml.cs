using ElectionGuard.UI.Helpers;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace ElectionGuard.UI;

public partial class AppShell
{
    public AppShell()
    {
        InitializeComponent();
        
        var navigationService = Ioc.Default.GetInstance<INavigationService> ();
        navigationService.RegisterRoutes();
    }
}
