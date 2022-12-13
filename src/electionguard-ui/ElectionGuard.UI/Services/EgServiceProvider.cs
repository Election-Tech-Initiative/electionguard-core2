using CommunityToolkit.Mvvm.DependencyInjection;
using ElectionGuard.UI.Lib.Services;

namespace ElectionGuard.UI.Services;

public class EgServiceProvider : IMauiInitializeService
{
    public void Initialize(IServiceProvider services)
    {
        Ioc.Default.ConfigureServices(services);
    }
}
