using ElectionGuard.UI.Lib.Models;
using ElectionGuard.UI.ViewModels;
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
            var createKeyCeremonyAdminViewModel = CreateKeyCeremonyAdminViewModel();
            _keyCeremonyService.Create(Arg.Any<KeyCeremony>()).Returns(42);

            // ACT
            await createKeyCeremonyAdminViewModel.CreateKeyCeremonyCommand.ExecuteAsync(null);

            // ASSERT
            await NavigationService.Received().GoToPage(
                typeof(ViewKeyCeremonyViewModel), 
                Arg.Is<Dictionary<string, object>>(dict => (int)dict["KeyCeremonyId"] == 42));
        }

        [Test]
        public void GivenQuorumIsGreaterThanNumGuardians_WhenTryToCreate_ThenCanNotCreate()
        {
            // ARRANGE
            var createKeyCeremonyAdminViewModel = CreateKeyCeremonyAdminViewModel();
            createKeyCeremonyAdminViewModel.KeyCeremonyName = "A";
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
            var createKeyCeremonyAdminViewModel = CreateKeyCeremonyAdminViewModel();
            createKeyCeremonyAdminViewModel.KeyCeremonyName = "A";
            createKeyCeremonyAdminViewModel.Quorum = 3;
            createKeyCeremonyAdminViewModel.NumberOfGuardians = 3;

            // ACT
            var canExecute = createKeyCeremonyAdminViewModel.CreateKeyCeremonyCommand.CanExecute(null);
            
            // ASSERT
            canExecute.ShouldBeTrue();
        }

        [Test]
        public void GivenNameIsSpaces_WhenTryToCreate_ThenCanNotCreate()
        {
            // ARRANGE
            var createKeyCeremonyAdminViewModel = CreateKeyCeremonyAdminViewModel();
            createKeyCeremonyAdminViewModel.KeyCeremonyName = " ";

            // ACT
            var canExecute = createKeyCeremonyAdminViewModel.CreateKeyCeremonyCommand.CanExecute(null);
            
            // ASSERT
            canExecute.ShouldBeFalse();
        }

        [Test]
        public void GivenNameIsNonEmpty_WhenTryToCreate_ThenCanCreate()
        {
            // ARRANGE
            var createKeyCeremonyAdminViewModel = CreateKeyCeremonyAdminViewModel();
            createKeyCeremonyAdminViewModel.KeyCeremonyName = "A";

            // ACT
            var canExecute = createKeyCeremonyAdminViewModel.CreateKeyCeremonyCommand.CanExecute(null);
            
            // ASSERT
            canExecute.ShouldBeTrue();
        }

        [Test]
        public async Task GivenExistingKeyCeremony_WhenCreating_ThenErrorMessageIsSet()
        {
            // ARRANGE
            var createKeyCeremonyAdminViewModel = CreateKeyCeremonyAdminViewModel();
            createKeyCeremonyAdminViewModel.KeyCeremonyName = "kc1";
            _keyCeremonyService.FindByName("kc1").Returns(new KeyCeremony("kc1", 1, 1));

            // ACT
            await createKeyCeremonyAdminViewModel.CreateKeyCeremonyCommand.ExecuteAsync(null);

            // ASSERT
            createKeyCeremonyAdminViewModel.ErrorMessage.ShouldNotBeNull();
        }

        private CreateKeyCeremonyAdminViewModel CreateKeyCeremonyAdminViewModel()
        {
            return new CreateKeyCeremonyAdminViewModel(ServiceProvider, _keyCeremonyService);
        }
    }
}
