using System;
using ElectionGuard.UI.Lib.Services;

namespace ElectionGuard.UI.Services
{
	public class NavigationService : INavigationService
	{
		public NavigationService()
		{
		}

        public bool CanGoHome()
        {
            {
                return (Page is not null) && 
                    (Page is not LoginPage) && 
                    (Page is not AdminHomePage) && 
                    (Page is not GuardianHomePage);
            }
        }

        public BaseViewModel GetCurrentViewModel()
        {
            throw new NotImplementedException();
        }

        public Task GoHome()
        {
            throw new NotImplementedException();
        }

        public Task GotoModal(Type type)
        {
            throw new NotImplementedException();
        }

        public Task GotoPage(Type type)
        {
            throw new NotImplementedException();
        }
    }
}

