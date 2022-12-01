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
            await Task.Yield();
        }

        private bool CanCreate()
        {
            // todo: validation
            return true;
        }
    }
}

