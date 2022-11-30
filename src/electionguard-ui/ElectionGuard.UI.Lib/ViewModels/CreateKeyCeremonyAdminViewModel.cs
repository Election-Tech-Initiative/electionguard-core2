using System;
namespace ElectionGuard.UI.Lib.ViewModels
{
	public partial class CreateKeyCeremonyAdminViewModel : BaseViewModel
	{
		private const string _pageName = "CreateKeyCeremony";

		public CreateKeyCeremonyAdminViewModel(IServiceProvider serviceProvider) : base(_pageName, serviceProvider)
		{
		}

		[ObservableProperty]
		private string _test = "Test";

	}
}

