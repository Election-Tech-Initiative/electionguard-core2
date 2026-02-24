using CommunityToolkit.Mvvm.DependencyInjection;

namespace ElectionGuard.UI.Services;

public class EgServiceProvider : IMauiInitializeService
{
    public void Initialize(IServiceProvider services)
    {
        Ioc.Default.ConfigureServices(services);
    }
}
