using CommunityToolkit.Maui;
using ElectionGuard.UI.Services;
using Microsoft.Extensions.Logging;

namespace ElectionGuard.UI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var DbHost = Preferences.Get("DbAddress", "127.0.0.1");
        var DbPassword = Preferences.Get("DbPassword", "");
        DbService.Init(DbHost, DbPassword);

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
        //builder.Logging.add();
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
        // setup services
        builder.Services.AddSingleton<IMauiInitializeService>(new EgServiceProvider());
        builder.Services.AddTransient<IAuthenticationService, AuthenticationService>();
        builder.Services.AddTransient<IConfigurationService, ConfigurationService>();
        // LocalizationService has to be singleton because it uses events to propagate changes to language changed
        builder.Services.AddSingleton<ILocalizationService, LocalizationService>();
        // NavigationService has to be singleton because it stores the current page and vm
        builder.Services.AddSingleton<INavigationService, NavigationService>();

        // setup database services
        builder.Services.AddTransient<KeyCeremonyService>();
        builder.Services.AddTransient<GuardianPublicKeyService>();
        builder.Services.AddTransient<GuardianBackupService>();
        builder.Services.AddTransient<VerificationService>();
        builder.Services.AddTransient<UserService>();
        builder.Services.AddTransient<ElectionService>();
        builder.Services.AddTransient<TallyService>();
        builder.Services.AddTransient<ManifestService>();
        builder.Services.AddTransient<ContextService>();
        builder.Services.AddTransient<ConstantsService>();

        // setup view models
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<GuardianHomeViewModel>();
        builder.Services.AddTransient<AdminHomeViewModel>();
        builder.Services.AddTransient<ElectionViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<CreateKeyCeremonyAdminViewModel>();
        builder.Services.AddTransient<ViewKeyCeremonyViewModel>();
        builder.Services.AddTransient<CreateElectionViewModel>();

        // setup views
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<GuardianHomePage>();
        builder.Services.AddTransient<AdminHomePage>();
        builder.Services.AddTransient<ElectionPage>();
        builder.Services.AddTransient<CreateKeyCeremonyAdminPage>();
        builder.Services.AddTransient<ViewKeyCeremonyPage>();
        builder.Services.AddTransient<CreateElectionAdminPage>();

        // popup pages
        builder.Services.AddTransient<SettingsPage>();

        return builder;
    }


}
