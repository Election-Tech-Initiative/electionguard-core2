using System;
using ElectionGuard.UI.ViewModels;

namespace ElectionGuard.UI.Lib.Services
{
	public interface INavigationService
	{
		Task GotoPage(Type type);
		Task GotoModal(Type type);
        BaseViewModel GetCurrentViewModel();
        Task GoHome();

		bool CanGoHome();
    }
}

