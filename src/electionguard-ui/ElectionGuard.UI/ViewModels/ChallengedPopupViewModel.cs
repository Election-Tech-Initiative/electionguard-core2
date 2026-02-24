using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.Input;
using ElectionGuard.Decryption.Tally;
using MongoDB.Driver;
using Newtonsoft.Json;

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
    private BallotUploadService _ballotUploadService;
    private CiphertextTallyService _ciphertextTallyService;

    public ChallengedPopupViewModel(
        IServiceProvider serviceProvider,
        BallotService ballotService,
        BallotUploadService ballotUploadService,
        CiphertextTallyService ciphertextTallyService) :
        base(null, serviceProvider)
    {
        _ballotService = ballotService;
        _ballotUploadService = ballotUploadService;
        _ciphertextTallyService = ciphertextTallyService;
    }

    partial void OnElectionIdChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _ = Shell.Current.CurrentPage.Dispatcher.DispatchAsync(async () =>
            {
                SearchText = string.Empty;
                await SetElection(value);
            });
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
            // update the ballot data to spoiled
            var ballot = JsonConvert.DeserializeObject<CiphertextBallot>(CurrentBallot.BallotData!);
            ballot!.Spoil();
            CurrentBallot.BallotData = ballot.ToJson();
            _ = await _ballotService.SaveAsync(CurrentBallot);

            // change the ballot record to spoiled
            await _ballotService.ConvertToSpoiledByBallotCodeAsync(CurrentBallot.BallotCode!);

            // remove ballot from the list of challenged ballots
            _ = _challengedBallots.Remove(CurrentBallot);
            SearchText = string.Empty;

            // reset the filtered list back to the entire list
            FilteredBallotList = _challengedBallots.ToObservableCollection();

            // change the upload record counts
            await _ballotUploadService.DecrementBallotsChallenged(CurrentBallot.UploadId!);
            var tally = await _ciphertextTallyService.GetByUploadIdIdAsync(CurrentBallot.UploadId!);
            var tallyData = JsonConvert.DeserializeObject<CiphertextTally>(tally?.CiphertextTallyData!);

            // remove the ballot id from challenged list and add to spoined list in tally
            _ = tallyData!.ChallengedBallotIds.Remove(ballot!.ObjectId);
            _ = tallyData!.SpoiledBallotIds.Add(ballot!.ObjectId);
            tally.CiphertextTallyData = tallyData.ToJson();
            _ = await _ciphertextTallyService.SaveAsync(tally);
        }
    }

    public async Task SetElection(string electionId)
    {
        var challengedBallotCursor = await _ballotService.GetCursorBallotsByElectionIdStateAsync(electionId, BallotBoxState.Challenged);
        _challengedBallots = challengedBallotCursor.ToList();
        FilteredBallotList = _challengedBallots.ToObservableCollection();
    }
}
