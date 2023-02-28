using ElectionGuard.UI.ViewModels;
using NSubstitute;
using Shouldly;

namespace ElectionGuard.UI.Test.ViewModels
{
    public class ViewKeyCeremonyViewModelTest : TestBase
    {
        private readonly KeyCeremonyService _keyCeremonyService;
        private readonly GuardianPublicKeyService _guardianPublicKeyService;
        private readonly GuardianBackupService _guardianBackupService;
        private readonly VerificationService _verificationService;

        public ViewKeyCeremonyViewModelTest()
        {
            _keyCeremonyService = Substitute.For<KeyCeremonyService>();
            _guardianPublicKeyService = Substitute.For<GuardianPublicKeyService>();
            _guardianBackupService = Substitute.For<GuardianBackupService>();
            _verificationService = Substitute.For<VerificationService>();
        }

        [Test]
        public void GivenUserIsAdmin_WhenUserNavigatesToViewKeyCeremony_ThenJoinButtonIsNotVisible()
        {
            // ARRANGE
            AuthenticationService.IsAdmin.Returns(true);
            var viewKeyCeremonyViewModel = new ViewKeyCeremonyViewModel(ServiceProvider,
                                                                        _keyCeremonyService,
                                                                        _guardianPublicKeyService,
                                                                        _guardianBackupService,
                                                                        _verificationService);

            // ACT
            var isJoinVisible = viewKeyCeremonyViewModel.IsJoinVisible;

            // ASSERT
            Assert.That(isJoinVisible, Is.False);
            isJoinVisible.ShouldBeFalse();
        }

        [Test]
        public void GivenUserIsGuardian_WhenUserNavigatesToViewKeyCeremony_ThenJoinButtonIsVisible()
        {
            // ARRANGE
            AuthenticationService.IsAdmin.Returns(false);
            var viewKeyCeremonyViewModel = new ViewKeyCeremonyViewModel(ServiceProvider,
                                                                        _keyCeremonyService,
                                                                        _guardianPublicKeyService,
                                                                        _guardianBackupService,
                                                                        _verificationService);

            // ACT
            var isJoinVisible = viewKeyCeremonyViewModel.IsJoinVisible;

            // ASSERT
            isJoinVisible.ShouldBeTrue();
        }
    }
}
