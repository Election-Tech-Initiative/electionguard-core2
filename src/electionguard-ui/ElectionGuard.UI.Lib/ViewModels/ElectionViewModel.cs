using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.Lib.ViewModels;

public partial class ElectionViewModel : BaseViewModel
{
    public ElectionViewModel(
        ILocalizationService localizationService,
        INavigationService navigationService,
        IConfigurationService configurationService) 
        : base(null, localizationService, navigationService, configurationService)
    {
        PropertyChanged += ElectionViewModel_PropertyChanged;
    }

    private void ElectionViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CurrentElection))
        {
            PageTitle = CurrentElection?.Name ?? "";
        }
    }

    [ObservableProperty]
    private Election? _currentElection;

    public const string CurrentElectionParam = "CurrentElection";

    public override void Dispose()
    {
        base.Dispose();
        PropertyChanged -= ElectionViewModel_PropertyChanged;
    }
}
