using CommunityToolkit.Mvvm.Input;

namespace ElectionGuard.UI.ViewModels;

public partial class CreateMultiTallyViewModel : BaseViewModel
{
    private TallyService _tallyService;
    private ElectionService _electionService;
    private ManifestService _manifestService;
    private KeyCeremonyService _keyCeremonyService;
    private BallotUploadService _ballotUploadService;

    public CreateMultiTallyViewModel(IServiceProvider serviceProvider, TallyService tallyService, ManifestService manifestService, ElectionService electionService, KeyCeremonyService keyCeremonyService, BallotUploadService ballotUploadService) : base("CreateMultiTally", serviceProvider)
    {
        _tallyService = tallyService;
        _manifestService = manifestService;
        _electionService = electionService;
        _keyCeremonyService = keyCeremonyService;
        _ballotUploadService = ballotUploadService;
        _ = Task.Run(FillKeyCeremonies);
    }

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _selectAll;

    [ObservableProperty]
    private bool _electionsLoaded;

    [ObservableProperty]
    private ObservableCollection<KeyCeremonyRecord> _keyCeremonies = new();

    [ObservableProperty]
    private KeyCeremonyRecord _selectedKeyCeremony;

    [ObservableProperty]
    private ObservableCollection<ElectionItem> _elections = new();

    [ObservableProperty]
    private ObservableCollection<object> _selectedElections = new();

    partial void OnSelectAllChanged(bool value)
    {
        if (value)
        {
            foreach (var item in Elections)
            {
                if (!SelectedElections.Contains(item))
                {
                    SelectedElections.Add(item);
                }
            }
        }
    }

    partial void OnSelectedKeyCeremonyChanged(KeyCeremonyRecord value)
    {
        // fill in the list of the elections that use the current key ceremony
        _ = Task.Run(async () =>
        {
            var allElections = await _electionService.GetAllByKeyCeremonyIdAsync(value.KeyCeremonyId);
            foreach (var item in allElections)
            {
                var allUploads = await _ballotUploadService.GetByElectionIdAsync(item.ElectionId);
                if (allUploads.Count() > 0)
                {
                    var ballotCountTotal = 0L;
                    var ballotAddedTotal = 0L;
                    var ballotChallengedTotal = 0L;
                    var ballotSpoiledTotal = 0L;
                    var ballotDuplicateTotal = 0L;
                    var ballotRejectedTotal = 0L;

                    allUploads.ForEach((upload) =>
                    {
                        ballotCountTotal += upload.BallotCount;
                        ballotAddedTotal += upload.BallotImported;
                        ballotChallengedTotal += upload.BallotChallenged;
                        ballotSpoiledTotal += upload.BallotSpoiled;
                        ballotDuplicateTotal += upload.BallotDuplicated;
                        ballotRejectedTotal += upload.BallotRejected;
                    });
                    var election = new ElectionItem
                    {
                        Election = item,
                        BallotUploads = new(allUploads),
                        BallotAddedTotal = ballotAddedTotal,
                        BallotChallengedTotal = ballotChallengedTotal,
                        BallotDuplicateTotal = ballotDuplicateTotal,
                        BallotRejectedTotal = ballotDuplicateTotal,
                        BallotSpoiledTotal = ballotSpoiledTotal,
                        BallotCountTotal = ballotCountTotal
                    };
                    Elections.Add(election);
                }
            }
            ElectionsLoaded = true;
        });
    }

    private async Task FillKeyCeremonies()
    {
        var allKeys = await _keyCeremonyService.GetAllCompleteAsync();
        foreach (var item in allKeys)
        {
            var count = await _electionService.CountByKeyCeremonyIdAsync(item.KeyCeremonyId);
            if(count > 1)
            {
                KeyCeremonies.Add(item);
            }
        }
    }

    [RelayCommand]
    private void SelectionChanged()
    {
        if(SelectedElections.Count != Elections.Count)
        {
            SelectAll = false;
        }
        CreateTalliesCommand.NotifyCanExecuteChanged();
        JoinTalliesCommand.NotifyCanExecuteChanged();
    }


    [RelayCommand(CanExecute = nameof(TalliesSelected))]
    private void CreateTallies()
    {
    }

    [RelayCommand(CanExecute = nameof(TalliesSelected))]
    private void JoinTallies()
    {
    }

    private bool TalliesSelected() => SelectedElections.Count > 0;

}

