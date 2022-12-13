using ElectionGuard.UI.ViewModels;
using NSubstitute;

namespace ElectionGuard.UI.Test.ViewModels;

public class AdminHomeViewModelTest : TestBase
{
    [Test]
    public async Task Given_WhenKeyCeremonyButtonClicked_ThenNavToCeremony()
    {
        // ARRANGE
        var adminHomeViewModel = new AdminHomeViewModel(ServiceProvider);

        // ACT
        await adminHomeViewModel.GoKeyCeremonyCommand.ExecuteAsync(null);

        // ASSERT
        await NavigationService.Received().GoToPage(typeof(CreateKeyCeremonyAdminViewModel));
    }
}

