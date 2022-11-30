namespace ElectionGuard.UI.Lib.Services
{
    public interface INavigationService
    {
        Task GoToPage(Type viewModel, Dictionary<string, object>? pageParams = null);
        Task GoToModal(Type viewModel);
        Type GetCurrentViewModel();
        Task GoHome();

        bool CanGoHome();
        void RegisterRoutes();
    }
}

