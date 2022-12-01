using CommunityToolkit.Mvvm.Input;
using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.Lib.ViewModels
{
    public partial class CreateKeyCeremonyAdminViewModel : BaseViewModel
    {
        private readonly IKeyCeremonyService _keyCeremonyService;
        private const string PageName = "CreateKeyCeremony";

        public CreateKeyCeremonyAdminViewModel(IServiceProvider serviceProvider, IKeyCeremonyService keyCeremonyService) : base(PageName, serviceProvider)
        {
            _keyCeremonyService = keyCeremonyService;
        }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CreateKeyCeremonyCommand))]
        private string _keyCeremonyName = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CreateKeyCeremonyCommand))]
        private int _numberOfGuardians = 3;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CreateKeyCeremonyCommand))]
        private int _quorum = 3;

        [RelayCommand(CanExecute = nameof(CanCreate))]
        public async Task CreateKeyCeremony()
        {
            // todo: check for duplicates

            var keyCeremony = new KeyCeremony(KeyCeremonyName, Quorum, NumberOfGuardians);
            var keyCeremonyId = _keyCeremonyService.Create(keyCeremony);
            await NavigationService.GoToPage(typeof(ViewKeyCeremonyViewModel), new Dictionary<string, object>
            {
                { ViewKeyCeremonyViewModel.CurrentKeyCeremonyParam, keyCeremonyId }
            });
        }

        private bool CanCreate()
        {
            return KeyCeremonyName.Trim().Length > 0 &&
                Quorum <= NumberOfGuardians;
        }
    }
}

