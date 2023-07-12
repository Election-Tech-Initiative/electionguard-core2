using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.Input;
using MongoDB.Driver;

namespace ElectionGuard.UI.ViewModels;

public partial class ChallengedPopupViewModel : BaseViewModel
{
    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<BallotRecord> _filteredBallotList = new();

    [ObservableProperty]
    private BallotRecord? _currentBallot = null;

    [ObservableProperty]
    private string _electionId = string.Empty;

    private List<BallotRecord> _challengedBallots = new();
    private BallotService _ballotService;

    public ChallengedPopupViewModel(
        IServiceProvider serviceProvider,
        BallotService ballotService) :
        base(null, serviceProvider)
    {
        _ballotService = ballotService;
    }

    partial void OnElectionIdChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _ = Shell.Current.CurrentPage.Dispatcher.DispatchAsync(async () => await SetElection(value));
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        FilteredBallotList = string.IsNullOrWhiteSpace(value) ?
            _challengedBallots.ToObservableCollection() :
            _challengedBallots.Where(b => b.BallotCode!.StartsWith(value.ToUpper())).ToObservableCollection();
    }

    [RelayCommand]
    private async Task SpoilBallot()
    {
        if (CurrentBallot is null)
        {
            return;
        }
        var answer = await Shell.Current.CurrentPage.DisplayAlert(AppResources.SpoilBallotText, AppResources.SpoilConfirmationText, AppResources.YesText, AppResources.NoText);
        if (answer)
        {
            await _ballotService.ConvertToSpoiledByBallotCodeAsync(CurrentBallot.BallotCode!);
            _ = _challengedBallots.Remove(CurrentBallot);
            SearchText = string.Empty;
        }
    }

    public async Task SetElection(string electionId)
    {
        var challengedBallotCursor = await _ballotService.GetCursorBallotsByElectionIdStateAsync(electionId, BallotBoxState.Challenged);
        _challengedBallots = challengedBallotCursor.ToList();
        FilteredBallotList = _challengedBallots.ToObservableCollection();
    }
}
