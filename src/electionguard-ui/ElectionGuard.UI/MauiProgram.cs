using CommunityToolkit.Maui;
using ElectionGuard.Decryption;
using ElectionGuard.UI.Services;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Analytics;
using MetroLog.MicrosoftExtensions;
using System.Reflection;

namespace ElectionGuard.UI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {

        if (!string.IsNullOrEmpty(DbContext.DbConnection))
        {
            DbService.Init(DbContext.DbConnection);
        }
        else
        {
            DbService.Init(DbContext.DbHost, DbContext.DbPassword);
        }

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .SetupFonts()
            .SetupServices()
            .SetupLogging()
            .ConfigureEssentials(essentials =>
            {
                essentials.UseVersionTracking();
            });


        return builder.Build();
    }

    public static MauiAppBuilder SetupLogging(this MauiAppBuilder builder)
    {
#if DEBUG
        builder.Logging.AddDebug();
        //builder.Logging.add();
#endif
        builder.Logging.AddStreamingFileLogger(configure =>
        {
            configure.FolderPath = ErrorLog.CreateLogPath();
        });
        return builder;
    }


    public static MauiAppBuilder SetupFonts(this MauiAppBuilder builder)
    {
        builder.ConfigureFonts(fonts =>
        {
            fonts.AddFont("opensans_regular.ttf", "OpenSansRegular");
            fonts.AddFont("opensans_semibold.ttf", "OpenSansSemibold");
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
        builder.Services.AddTransient<IStorageService, DriveService>();
        builder.Services.AddTransient<ZipStorageService>();

        // setup database services
        builder.Services.AddTransient<KeyCeremonyService>();
        builder.Services.AddTransient<GuardianPublicKeyService>();
        builder.Services.AddTransient<GuardianBackupService>();
        builder.Services.AddTransient<VerificationService>();
        builder.Services.AddTransient<UserService>();
        builder.Services.AddTransient<ElectionService>();
        builder.Services.AddTransient<TallyService>();
        builder.Services.AddTransient<TallyJoinedService>();
        builder.Services.AddTransient<ManifestService>();
        builder.Services.AddTransient<ContextService>();
        builder.Services.AddTransient<ConstantsService>();
        builder.Services.AddTransient<BallotUploadService>();
        builder.Services.AddTransient<BallotService>();
        builder.Services.AddTransient<ChallengedBallotService>();
        builder.Services.AddTransient<CiphertextTallyService>();
        builder.Services.AddTransient<TallyMediator>();
        builder.Services.AddTransient<DecryptionShareService>();
        builder.Services.AddTransient<ChallengeService>();
        builder.Services.AddTransient<ChallengeResponseService>();
        builder.Services.AddTransient<PlaintextTallyService>();
        builder.Services.AddTransient<LagrangeCoefficientsService>();
        builder.Services.AddTransient<TallyManager>();
        builder.Services.AddTransient<ITallyStateMachine, TallyStateMachine>();
        builder.Services.AddTransient<MultiTallyService>();

        // setup view models
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<GuardianHomeViewModel>();
        builder.Services.AddTransient<AdminHomeViewModel>();
        builder.Services.AddSingleton<ElectionViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<CreateKeyCeremonyAdminViewModel>();
        builder.Services.AddTransient<ViewKeyCeremonyViewModel>();
        builder.Services.AddTransient<CreateElectionViewModel>();
        builder.Services.AddSingleton<ManifestViewModel>();
        builder.Services.AddSingleton<ChallengedPopupViewModel>();
        builder.Services.AddTransient<BallotUploadViewModel>();
        builder.Services.AddTransient<CreateTallyViewModel>();
        builder.Services.AddTransient<CreateMultiTallyViewModel>();
        builder.Services.AddTransient<TallyProcessViewModel>();
        builder.Services.AddTransient<ViewTallyViewModel>();

        // setup views
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<GuardianHomePage>();
        builder.Services.AddTransient<AdminHomePage>();
        builder.Services.AddTransient<ElectionPage>();
        builder.Services.AddTransient<CreateKeyCeremonyAdminPage>();
        builder.Services.AddTransient<ViewKeyCeremonyPage>();
        builder.Services.AddTransient<CreateElectionAdminPage>();
        builder.Services.AddTransient<ManifestPopup>();
        builder.Services.AddTransient<ChallengedPopup>();
        builder.Services.AddTransient<BallotUploadPage>();
        builder.Services.AddTransient<CreateTallyPage>();
        builder.Services.AddTransient<CreateMultiTallyPage>();
        builder.Services.AddTransient<TallyProcessPage>();
        builder.Services.AddTransient<ViewTallyPage>();

        // popup pages
        builder.Services.AddTransient<SettingsPage>();

        return builder;
    }
}
