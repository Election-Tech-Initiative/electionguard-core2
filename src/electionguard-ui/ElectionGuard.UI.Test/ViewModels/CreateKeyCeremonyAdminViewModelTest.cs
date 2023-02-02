using ElectionGuard.UI.Lib.Models;
using ElectionGuard.UI.ViewModels;
using NSubstitute;
using Shouldly;

namespace ElectionGuard.UI.Test.ViewModels
{
    public class CreateKeyCeremonyAdminViewModelTest : TestBase
    {
        private readonly KeyCeremonyService _keyCeremonyService;

        public CreateKeyCeremonyAdminViewModelTest()
        {
            _keyCeremonyService = Substitute.For<KeyCeremonyService>();
        }

        [Test]
        public async Task GivenKeyCeremonyReturnsId_WhenUserCreatesKeyCeremony_ThenUserNavigatesToViewPageWithId()
        {
            // ARRANGE
            var createKeyCeremonyAdminViewModel = CreateKeyCeremonyAdminViewModel();
            var ret = new KeyCeremony("test", 1, 1, "admin");
            _keyCeremonyService.SaveAsync(Arg.Any<KeyCeremony>()).Returns(ret);

            // ACT
            await createKeyCeremonyAdminViewModel.CreateKeyCeremonyCommand.ExecuteAsync(null);

            // ASSERT
            await NavigationService.Received().GoToPage(
                typeof(ViewKeyCeremonyViewModel),
                Arg.Is<Dictionary<string, object>>(dict => (string)dict["KeyCeremonyId"] == ret.Id));
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
            _keyCeremonyService.GetByNameAsync("kc1").Returns(new KeyCeremony("kc1", 1, 1, "admin"));

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
