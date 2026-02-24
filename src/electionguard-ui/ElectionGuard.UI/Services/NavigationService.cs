using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace ElectionGuard.UI.Services;

public record PageType(
    Type ViewModel,
    Type Page,
    bool IsGlobal
);

public class NavigationService : INavigationService
{
    private static readonly List<PageType> PageTypes = new()
    {
        new PageType(typeof(LoginViewModel), typeof(LoginPage), true),
        new PageType(typeof(SettingsViewModel), typeof(SettingsPage), true),
        new PageType(typeof(AdminHomeViewModel), typeof(AdminHomePage), true),
        new PageType(typeof(GuardianHomeViewModel), typeof(GuardianHomePage), true),
        new PageType(typeof(ElectionViewModel), typeof(ElectionPage), false),
        new PageType(typeof(CreateKeyCeremonyAdminViewModel), typeof(CreateKeyCeremonyAdminPage), false),
        new PageType(typeof(ViewKeyCeremonyViewModel), typeof(ViewKeyCeremonyPage), false),
        new PageType(typeof(CreateElectionViewModel), typeof(CreateElectionAdminPage), false),
        new PageType(typeof(ManifestViewModel), typeof(ManifestPopup), false),
        new PageType(typeof(BallotUploadViewModel), typeof(BallotUploadPage), false),
        new PageType(typeof(CreateTallyViewModel), typeof(CreateTallyPage), false),
        new PageType(typeof(CreateMultiTallyViewModel), typeof(CreateMultiTallyPage), false),
        new PageType(typeof(TallyProcessViewModel), typeof(TallyProcessPage), false),
        new PageType(typeof(ViewTallyViewModel), typeof(ViewTallyPage), false),
        new PageType(typeof(ChallengedPopupViewModel), typeof(ChallengedPopup), false),
    };

    private Type _currentPage = typeof(LoginPage);
    private Type _currentViewModel = typeof(LoginViewModel);

    public bool CanGoHome()
    {
        return !_currentPage.IsAssignableFrom(typeof(LoginPage)) &&
               !_currentPage.IsAssignableFrom(typeof(AdminHomePage)) &&
               !_currentPage.IsAssignableFrom(typeof(GuardianHomePage));
    }

    public void RegisterRoutes()
    {
        foreach (var pageType in PageTypes)
        {
            Routing.RegisterRoute(pageType.Page.Name, pageType.Page);
        }
    }

    public Type GetCurrentViewModel()
    {
        return _currentViewModel;
    }

    public async Task GoHome()
    {
        var page = GetHomePage();
        await GoToPage(page);
    }

    private static Type GetHomePage()
    {
        if (App.CurrentUser.IsAdmin)
            return typeof(AdminHomeViewModel);
        return typeof(GuardianHomeViewModel);
    }

    public async Task GoToModal(Type viewModel)
    {
        var pageType = GetPage(viewModel);
        var pageInstance = GetPopupInstance(pageType.Page);
        await Shell.Current.CurrentPage.ShowPopupAsync(pageInstance);
    }

    private static Popup GetPopupInstance(Type type)
    {
        var popup = Ioc.Default.GetService(type);
        if (popup != null) return (Popup)popup;
        throw new ArgumentException(
            $"The type {type} isn't registered in the service collection, set it in MauiProgram.cs");
    }

    public async Task GoToPage(Type viewModel, Dictionary<string, object>? pageParams = null)
    {
        _currentViewModel = viewModel;
        var contentPage = GetPage(viewModel);
        _currentPage = contentPage.Page;
        var url = contentPage.IsGlobal ? $"//{_currentPage.Name}" : _currentPage.Name;
        await Shell.Current.GoToAsync(url, pageParams ?? new());
    }

    public async Task GoBack()
    {
        await Shell.Current.GoToAsync("..");
    }

    private static PageType GetPage(Type viewModel)
    {
        var pageType = PageTypes.FirstOrDefault(i => i.ViewModel == viewModel);
        if (pageType != null)
            return pageType;

        throw new ArgumentException($"ViewModel mapping not found, add {viewModel} to NavigationService.ViewModelMappings");
    }
}
