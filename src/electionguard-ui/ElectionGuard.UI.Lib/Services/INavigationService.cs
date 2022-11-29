namespace ElectionGuard.UI.Lib.Services
{
	public interface INavigationService
	{
		Task GoToPage(Type type);
		Task GoToModal(Type type);
        Type GetCurrentViewModel();
        Task GoHome();

		bool CanGoHome();
    }
}

