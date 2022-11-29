using CommunityToolkit.Maui;
using ElectionGuard.UI.Lib.Services;
using ElectionGuard.UI.Services;
using Microsoft.Extensions.Logging;

namespace ElectionGuard.UI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
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
        // services
        builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();
        builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
        builder.Services.AddSingleton<ILocalizationService, LocalizationService>();
        builder.Services.AddSingleton<INavigationService, NavigationService>();

        // setup view models
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
