using ElectionGuard.UI.Lib.Models;
using ElectionGuard.UI.Lib.ViewModels;
using NSubstitute;
using Shouldly;

namespace ElectionGuard.UI.Test.ViewModels
{
    public class CreateKeyCeremonyAdminViewModelTest : TestBase
    {
        private readonly IKeyCeremonyService _keyCeremonyService;

        public CreateKeyCeremonyAdminViewModelTest()
        {
            _keyCeremonyService = Substitute.For<IKeyCeremonyService>();
        }

        [Test]
        public async Task GivenKeyCeremonyReturnsId_WhenUserCreatesKeyCeremony_ThenUserNavigatesToViewPageWithId()
        {
            // ARRANGE
            var createKeyCeremonyAdminViewModel = new CreateKeyCeremonyAdminViewModel(ServiceProvider, _keyCeremonyService);
            _keyCeremonyService.Create(Arg.Any<KeyCeremony>()).Returns(42);

            // ACT
            await createKeyCeremonyAdminViewModel.CreateKeyCeremonyCommand.ExecuteAsync(null);

            // ASSERT
            await NavigationService.Received().GoToPage(
                typeof(ViewKeyCeremonyViewModel), 
                Arg.Is<Dictionary<string, object>>(dict => (int)dict["KeyCeremonyId"] == 42));
        }

        [Test]
        public void GivenQuorumIsGreaterThanNumGuardians_WhenTryToCreate_ThenCantCreate()
        {
            // ARRANGE
            var createKeyCeremonyAdminViewModel = new CreateKeyCeremonyAdminViewModel(ServiceProvider, _keyCeremonyService);
            createKeyCeremonyAdminViewModel.Quorum = 3;
            createKeyCeremonyAdminViewModel.NumberOfGuardians = 2;

            // ACT
            var canExecute = createKeyCeremonyAdminViewModel.CreateKeyCeremonyCommand.CanExecute(null);
            
            // ASSERT
            canExecute.ShouldBeFalse();
        }

        [Test]
        public void GivenQuorumIsEqualToNumGuardians_WhenTryToCreate_ThenCanCreate()
        {
            // ARRANGE
            var createKeyCeremonyAdminViewModel = new CreateKeyCeremonyAdminViewModel(ServiceProvider, _keyCeremonyService);
            createKeyCeremonyAdminViewModel.Quorum = 3;
            createKeyCeremonyAdminViewModel.NumberOfGuardians = 3;

            // ACT
            var canExecute = createKeyCeremonyAdminViewModel.CreateKeyCeremonyCommand.CanExecute(null);
            
            // ASSERT
            canExecute.ShouldBeTrue();
        }
    }
}
