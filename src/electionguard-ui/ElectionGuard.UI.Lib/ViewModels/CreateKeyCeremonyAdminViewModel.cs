using CommunityToolkit.Mvvm.Input;

namespace ElectionGuard.UI.Lib.ViewModels
{
	public partial class CreateKeyCeremonyAdminViewModel : BaseViewModel
	{
		private const string PageName = "CreateKeyCeremony";

		public CreateKeyCeremonyAdminViewModel(IServiceProvider serviceProvider) : base(PageName, serviceProvider)
		{
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
            // todo: save to database
            // todo: redirect to key ceremony detail page
            // var keyCeremonyId = KeyCeremonyService.Create();
            await NavigationService.GoToPage(typeof(ViewKeyCeremonyViewModel), new Dictionary<string, object>
            {
                { "KeyCeremonyId", 22 }
            });
        }

        private bool CanCreate()
        {
            // todo: validate quorum isn't greater than # of guardians
            return true;
        }
    }
}

