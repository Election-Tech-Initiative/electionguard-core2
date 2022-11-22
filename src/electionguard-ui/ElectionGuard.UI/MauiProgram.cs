using ElectionGuard.UI.ViewModels;
using Microsoft.Extensions.Logging;

namespace ElectionGuard.UI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .SetupFonts()
            .SetupServices()
            .SetupLogging();

        return builder.Build();
    }

    public static MauiAppBuilder SetupLogging(this MauiAppBuilder builder)
    {
#if DEBUG
		builder.Logging.AddDebug();
#endif
        return builder;
    }


    public static MauiAppBuilder SetupFonts(this MauiAppBuilder builder)
    {
        builder.ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        });

        return builder;
    }

    public static MauiAppBuilder SetupServices(this MauiAppBuilder builder)
    {
        // setup viewmodels
        builder.Services.AddSingleton<MainViewModel>();

        // setup views
        builder.Services.AddSingleton<MainPage>();

        return builder;
    }


}
