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
        builder.Services.AddSingleton((_) => EgServiceProvider.Current);
        builder.Services.AddTransient<IAuthenticationService, AuthenticationService>();
        builder.Services.AddTransient<IConfigurationService, ConfigurationService>();
        // LocalizationService has to be singleton because it uses events to propagate changes to language changed
        builder.Services.AddSingleton<ILocalizationService, LocalizationService>();
        // NavigationService has to be singleton because it stores the current page and vm
        builder.Services.AddSingleton<INavigationService, NavigationService>();

        // setup view models
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<GuardianHomeViewModel>();
        builder.Services.AddTransient<AdminHomeViewModel>();
        builder.Services.AddTransient<ElectionViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<CreateKeyCeremonyAdminViewModel>();

        // setup views
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<GuardianHomePage>();
        builder.Services.AddTransient<AdminHomePage>();
        builder.Services.AddTransient<ElectionPage>();
        builder.Services.AddTransient<CreateKeyCeremonyAdminPage>();

        // popup pages
        builder.Services.AddTransient<SettingsPage>();

        return builder;
    }


}
