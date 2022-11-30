namespace ElectionGuard.UI.Services
{
    public static class ServiceProvider
    {
        //public static TService GetServiceTService<TService>()
        //    => Current.GetService<TService>();

        public static IServiceProvider Current
            =>
#if MACCATALYST
                    MauiUIApplicationDelegate.Current.Services;
#else
        MauiWinUIApplication.Current.Services;
#endif
    }
}
