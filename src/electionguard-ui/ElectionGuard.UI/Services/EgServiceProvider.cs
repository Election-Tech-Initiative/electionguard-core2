using ElectionGuard.UI.Lib.Services;

namespace ElectionGuard.UI.Services;

public class EgServiceProvider
{
    public static IServiceProvider Current
        =>
#if MACCATALYST
            MauiUIApplicationDelegate.Current.Services;
#else
            MauiWinUIApplication.Current.Services;
#endif
}
