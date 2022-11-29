using CommunityToolkit.Maui;
using ElectionGuard.UI.Lib.Services;
using Microsoft.Extensions.Logging;

namespace ElectionGuard.UI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        DbService.Init("locahost", "testing");
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
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
        builder.Services.AddSingleton<LoginViewModel>();
        builder.Services.AddSingleton<GuardianHomeViewModel>();
        builder.Services.AddSingleton<AdminHomeViewModel>();
        builder.Services.AddSingleton<ElectionViewModel>();

        // setup views
        builder.Services.AddSingleton<LoginPage>();
        builder.Services.AddSingleton<GuardianHomePage>();
        builder.Services.AddSingleton<AdminHomePage>();
        builder.Services.AddSingleton<ElectionPage>();

        return builder;
    }


}
