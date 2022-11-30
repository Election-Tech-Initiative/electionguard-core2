using CommunityToolkit.Maui.Views;
using ElectionGuard.UI.Lib.Services;

namespace ElectionGuard.UI.Services
{
    public record PageType(
        Type ViewModel,
        Type Page,
        bool IsGlobal
    );
    
    public class NavigationService : INavigationService
    {
        private static readonly List<PageType> ViewModelMappings = new()
        {
            new PageType(typeof(LoginViewModel), typeof(LoginPage), true),
            new PageType(typeof(SettingsViewModel), typeof(SettingsViewModel), true),
            new PageType(typeof(AdminHomeViewModel), typeof(AdminHomePage), true),
            new PageType(typeof(ElectionViewModel), typeof(ElectionPage), false)
        };

        private Type _currentPage = typeof(LoginPage);
        private Type _currentViewModel = typeof(LoginViewModel);

        public bool CanGoHome()
        {
            return !_currentPage.IsAssignableFrom(typeof(LoginPage)) &&
                   !_currentPage.IsAssignableFrom(typeof(AdminHomePage)) &&
                   !_currentPage.IsAssignableFrom(typeof(GuardianHomePage));
        }

        public Type GetCurrentViewModel()
        {
            return _currentViewModel;
        }

        public async Task GoHome()
        {
            if (App.CurrentUser.IsAdmin)
            {
                await GoToPage(typeof(AdminHomePage));
            }
            else
            {
                await Shell.Current.GoToAsync($"//{nameof(GuardianHomePage)}");
            }
        }

        public async Task GoToModal(Type viewModel)
        {
            var pageType = GetPage(viewModel);
            var pageInstance = GetPopupInstance(pageType.Page);
            await Shell.Current.CurrentPage.ShowPopupAsync(pageInstance);
        }

        private Popup GetPopupInstance(Type type)
        {
            var popup = ServiceProvider.Current.GetService(type);
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
            await Shell.Current.GoToAsync(url, pageParams);
        }

        private PageType GetPage(Type viewModel)
        {
            var pageType = ViewModelMappings.FirstOrDefault(i => i.ViewModel == viewModel);
            if (pageType != null)
                return pageType;

            throw new ArgumentException($"ViewModel mapping not found, add {viewModel} to NavigationService.ViewModelMappings");
        }
    }
}

