namespace ElectionGuard.UI.Lib.Services;

public interface INavigationService
{
    Task GoToPage(Type viewModel, Dictionary<string, object>? pageParams = null);
    Task GoToModal(Type viewModel);
    Type GetCurrentViewModel();
    Task GoHome();
    Task GoBack();

    bool CanGoHome();
    void RegisterRoutes();
}

