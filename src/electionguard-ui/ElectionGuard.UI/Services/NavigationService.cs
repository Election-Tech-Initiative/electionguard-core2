using CommunityToolkit.Maui.Views;
using ElectionGuard.UI.Lib.Services;

namespace ElectionGuard.UI.Services
{
    public class NavigationService : INavigationService
    {
        private static readonly Dictionary<Type, Type> ViewModelMappings = new()
        {
            { typeof(LoginViewModel), typeof(LoginPage) },
            { typeof(SettingsViewModel), typeof(SettingsPage) },
            { typeof(AdminHomeViewModel), typeof(AdminHomePage) },
            { typeof(ElectionViewModel), typeof(ElectionPage) }
        };

        private Type _currentPage = typeof(LoginPage);
        private Type _currentViewModel = typeof(LoginViewModel);
        private readonly List<Popup> _modals = new();

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
            var page = GetPage(viewModel);
            var pageInstance = GetPopupInstance(page);
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
            _currentPage = contentPage;
            await Shell.Current.GoToAsync($"//{_currentPage}", pageParams);
        }

        private Type GetPage(Type viewModel)
        {
            if (ViewModelMappings.TryGetValue(viewModel, out var page))
                return page;

            throw new ArgumentException($"ViewModel mapping not found, add {viewModel} to NavigationService.ViewModelMappings");
        }
    }
}

