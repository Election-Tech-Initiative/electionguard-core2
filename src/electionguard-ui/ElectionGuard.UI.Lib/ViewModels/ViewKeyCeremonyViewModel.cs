using CommunityToolkit.Mvvm.Input;
using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.Lib.ViewModels
{
    public partial class ViewKeyCeremonyViewModel : BaseViewModel
    {
        public const string CurrentKeyCeremonyParam = "KeyCeremonyId";

        public ViewKeyCeremonyViewModel(IServiceProvider serviceProvider, IKeyCeremonyService keyCeremonyService) : 
            base("ViewKeyCeremony", serviceProvider)
        {
            _keyCeremonyService = keyCeremonyService;
        }

        [ObservableProperty]
        private KeyCeremony? _keyCeremony;

        [RelayCommand]
        public async Task RetrieveKeyCeremony(int keyCeremonyId)
        {
            KeyCeremony = await _keyCeremonyService.Get(keyCeremonyId);
        }

        private readonly IKeyCeremonyService _keyCeremonyService;
    }
}
