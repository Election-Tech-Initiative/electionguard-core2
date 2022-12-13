using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.ViewModels;

[QueryProperty(CurrentElectionParam, nameof(CurrentElection))]
public partial class ElectionViewModel : BaseViewModel
{
    public ElectionViewModel(IServiceProvider serviceProvider) : base(null, serviceProvider)
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
        GC.SuppressFinalize(this);
    }
}
