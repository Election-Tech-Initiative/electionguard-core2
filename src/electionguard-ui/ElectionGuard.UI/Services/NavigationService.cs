using CommunityToolkit.Maui.Views;
using ElectionGuard.UI.Lib.Services;

namespace ElectionGuard.UI.Services
{
    public class NavigationService : INavigationService
    {
        public NavigationService(SettingsPage settingsPage)
        {
            _modals.Add(settingsPage);
        }

        private static readonly Dictionary<Type, Type> ViewModelMappings = new()
        {
            { typeof(LoginViewModel), typeof(LoginPage) },
            { typeof(SettingsViewModel), typeof(SettingsPage) },
            { typeof(AdminHomeViewModel), typeof(AdminHomePage) }
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
            var instance = _modals.FirstOrDefault(i => i.GetType() == type);
            if (instance == null)
                throw new ArgumentException($"Modal type {type} not found, add it to NavigationService._modals");
            return instance;
        }

        public async Task GoToPage(Type viewModel)
        {
            _currentViewModel = viewModel;
            var contentPage = GetPage(viewModel);
            _currentPage = contentPage;
            await Shell.Current.GoToAsync($"//{_currentPage}");
        }

        private Type GetPage(Type viewModel)
        {
            if (ViewModelMappings.TryGetValue(viewModel, out var page))
                return page;

            throw new ArgumentException($"ViewModel mapping not found, add {viewModel} to NavigationService.ViewModelMappings");
        }
    }
}

