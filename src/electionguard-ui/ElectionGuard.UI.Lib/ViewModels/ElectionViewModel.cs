using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.Lib.ViewModels;

public partial class ElectionViewModel : BaseViewModel
{
    public ElectionViewModel(
        ILocalizationService localizationService,
        INavigationService navigationService,
        IConfigurationService configurationService) : base(localizationService, navigationService, configurationService)
    {
    }

    [ObservableProperty]
    private Election? _currentElection;

    public const string CurrentElectionParam = "CurrentElection";
}
