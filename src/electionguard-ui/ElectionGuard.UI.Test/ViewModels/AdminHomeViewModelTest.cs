using ElectionGuard.UI.Lib.Services;
using ElectionGuard.UI.Lib.ViewModels;
using NSubstitute;

namespace ElectionGuard.UI.Test.ViewModels;

public class AdminHomeViewModelTest
{
    private readonly ILocalizationService _localizationService;
    private readonly INavigationService _navigationService;
    private readonly IConfigurationService _configurationService;

    public AdminHomeViewModelTest()
    {
        _localizationService = Substitute.For<ILocalizationService>();
        _navigationService = Substitute.For<INavigationService>();
        _configurationService = Substitute.For<IConfigurationService>();
    }

    [Test]
    public async Task Given_WhenKeyCeremonyButtonClicked_ThenNavToCeremony()
    {
        // ARRANGE
        var adminHomeViewModel = new AdminHomeViewModel(_localizationService, _navigationService, _configurationService);

        // ACT
        await adminHomeViewModel.GoKeyCeremonyCommand.ExecuteAsync(null);

        // ASSERT
        await _navigationService.Received().GoToPage(typeof(AdminHomeViewModel));
    }
}

