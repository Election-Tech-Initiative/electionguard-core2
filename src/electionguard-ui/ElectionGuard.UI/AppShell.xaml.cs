using ElectionGuard.UI.Lib.Services;
using ElectionGuard.UI.Services;
using ElectionGuard.UI.Lib.Extensions;
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
