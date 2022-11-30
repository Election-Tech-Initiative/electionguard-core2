using NSubstitute;

namespace ElectionGuard.UI.Test.ViewModels;

public abstract class TestBase
{
    protected readonly ILocalizationService LocalizationService;
    protected readonly INavigationService NavigationService;
    protected readonly IConfigurationService ConfigurationService;
    protected readonly IAuthenticationService AuthenticationService;
    protected readonly IServiceProvider ServiceProvider;

    protected TestBase()
    {
        LocalizationService = Substitute.For<ILocalizationService>();
        NavigationService = Substitute.For<INavigationService>();
        ConfigurationService = Substitute.For<IConfigurationService>();
        AuthenticationService = Substitute.For<IAuthenticationService>();
        ServiceProvider = Substitute.For<IServiceProvider>();
        ServiceProvider.GetService(typeof(IAuthenticationService)).Returns(AuthenticationService);
        ServiceProvider.GetService(typeof(INavigationService)).Returns(NavigationService);
        ServiceProvider.GetService(typeof(IConfigurationService)).Returns(ConfigurationService);
        ServiceProvider.GetService(typeof(ILocalizationService)).Returns(LocalizationService);
    }
}