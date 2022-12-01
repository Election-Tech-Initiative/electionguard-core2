using CommunityToolkit.Mvvm.Input;
using ElectionGuard.ElectionSetup;

namespace ElectionGuard.UI.Lib.ViewModels
{
    public partial class ViewKeyCeremonyViewModel : BaseViewModel
    {
        public const string CurrentKeyCeremonyParam = "KeyCeremonyId";

        public ViewKeyCeremonyViewModel(IServiceProvider serviceProvider, IKeyCeremonyService keyCeremonyService) : 
            base("ViewKeyCeremony", serviceProvider)
        {
            _keyCeremonyService = keyCeremonyService;
            IsJoinVisible = !AuthenticationService.IsAdmin;
        }

        [ObservableProperty]
        private Models.KeyCeremony? _keyCeremony;

        [ObservableProperty] 
        private bool _isJoinVisible;

        [RelayCommand]
        public async Task RetrieveKeyCeremony(int keyCeremonyId)
        {
            KeyCeremony = await _keyCeremonyService.Get(keyCeremonyId);
        }

        [RelayCommand]
        public void Join()
        {
            if (KeyCeremony == null) throw new ArgumentNullException(nameof(KeyCeremony));
            var currentGuardianUserName = AuthenticationService.UserName;
            var guardian = Guardian.FromNonce(currentGuardianUserName, 0, KeyCeremony.NumberOfGuardians, KeyCeremony.Quorum);
        }

        private readonly IKeyCeremonyService _keyCeremonyService;
    }
}
