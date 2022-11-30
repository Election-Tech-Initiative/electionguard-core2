using ElectionGuard.UI.Lib.Services;
using ElectionGuard.UI.Services;
using ElectionGuard.UI.Lib.Extensions;

namespace ElectionGuard.UI;

public partial class AppShell
{
    public AppShell()
    {
        InitializeComponent();
        
        var navigationService = EgServiceProvider.Current.GetInstance<INavigationService>();
        navigationService.RegisterRoutes();
    }
}
