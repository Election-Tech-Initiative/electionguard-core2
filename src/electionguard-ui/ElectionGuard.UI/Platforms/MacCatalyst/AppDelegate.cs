#pragma warning disable CA1711 // It's ok to name this a delegate
using Foundation;

namespace ElectionGuard.UI;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
