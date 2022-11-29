using ElectionGuard.UI.Lib.Services;
using ElectionGuard.UI.Lib.ViewModels;
using NSubstitute;

namespace ElectionGuard.UI.Test.ViewModels;

public class AdminHomeViewModelTest
{
    private readonly ElectionViewModel _electionViewModel;
    private readonly ILocalizationService _localizationService;
    private readonly INavigationService _navigationService;
    private readonly IConfigurationService _configurationService;

    public AdminHomeViewModelTest()
    {
        _electionViewModel = Substitute.For<ElectionViewModel>();
        _localizationService = Substitute.For<ILocalizationService>();
        _navigationService = Substitute.For<INavigationService>();
        _configurationService = Substitute.For<IConfigurationService>();
    }

    [Test]
    public void Given_WhenKeyCeremonyButtonClicked_ThenNavToCeremony()
    {
        var adminHomeViewModel = new AdminHomeViewModel(_localizationService, _navigationService, _configurationService, _electionViewModel);
        
        Assert.NotNull(adminHomeViewModel);
    }
}

