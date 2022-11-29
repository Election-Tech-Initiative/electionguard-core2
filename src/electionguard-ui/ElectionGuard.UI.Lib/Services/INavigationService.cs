namespace ElectionGuard.UI.Lib.Services
{
	public interface INavigationService
	{
		Task GoToPage(Type type, Dictionary<string, object>? pageParams = null);
		Task GoToModal(Type type);
        Type GetCurrentViewModel();
        Task GoHome();

		bool CanGoHome();
    }
}

