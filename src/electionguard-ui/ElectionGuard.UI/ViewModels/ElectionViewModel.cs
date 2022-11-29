namespace ElectionGuard.UI.ViewModels;

public partial class ElectionViewModel : BaseViewModel
{
    [ObservableProperty]
    private Election? _currentElection;
}
