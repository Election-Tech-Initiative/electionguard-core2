using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.Input;

namespace ElectionGuard.UI.ViewModels;

[QueryProperty(MultiTallyIdParam, nameof(MultiTallyId))]

public partial class CreateMultiTallyViewModel : BaseViewModel
{
    public const string MultiTallyIdParam = "MultiTallyId";

    private TallyService _tallyService;
    private ElectionService _electionService;
    private ManifestService _manifestService;
    private KeyCeremonyService _keyCeremonyService;
    private BallotUploadService _ballotUploadService;
    private BallotService _ballotService;
    private TallyJoinedService _tallyJoinedService;
    private MultiTallyService _multiTallyService;

    public CreateMultiTallyViewModel(
        IServiceProvider serviceProvider,
        TallyService tallyService,
        ManifestService manifestService,
        ElectionService electionService,
        KeyCeremonyService keyCeremonyService,
        BallotUploadService ballotUploadService,
        BallotService ballotService,
        TallyJoinedService tallyJoinedService,
        MultiTallyService multiTallyService) : base("CreateMultiTally", serviceProvider)
    {
        _tallyService = tallyService;
        _manifestService = manifestService;
        _electionService = electionService;
        _keyCeremonyService = keyCeremonyService;
        _ballotUploadService = ballotUploadService;
        _ballotService = ballotService;
        _tallyJoinedService = tallyJoinedService;
        _multiTallyService = multiTallyService;
        _ = Task.Run(FillKeyCeremonies);
    }

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _selectAll;

    [ObservableProperty]
    private string _multiTallyId = string.Empty;

    [ObservableProperty]
    private bool _electionsLoaded;

    [ObservableProperty]
    private ObservableCollection<KeyCeremonyRecord> _keyCeremonies = new();

    [ObservableProperty]
    private KeyCeremonyRecord _selectedKeyCeremony;

    [ObservableProperty]
    private MultiTallyRecord? _currentMultiTally;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateTalliesCommand))]
    [NotifyCanExecuteChangedFor(nameof(JoinTalliesCommand))]
    private string _currentResultsPath = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ElectionItem> _elections = new();

    [ObservableProperty]
    private ObservableCollection<object> _selectedElections = new();

    partial void OnMultiTallyIdChanged(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            CurrentMultiTally = null;
        }

        _ = Shell.Current.CurrentPage.Dispatcher.DispatchAsync(async () =>
        {
            // load the elections that are in the multitally
            var multiTally = await _multiTallyService.GetByMultiTallyIdAsync(value);
            if (multiTally != null)
            {
                foreach (var (tallyId, electionId, name) in multiTally.TallyIds)
                {
                    var election = await _electionService.GetByElectionIdAsync(electionId);
                    await LoadElectionData(election, tallyId);
                }
                ElectionsLoaded = true;
                CurrentMultiTally = multiTally;
                CurrentResultsPath = CurrentMultiTally.ResultsPath;
            }
        });
    }

    partial void OnSelectAllChanged(bool value)
    {
        if (value)
        {
            foreach (var election in Elections)
            {
                if (!SelectedElections.Contains(election))
                {
                    SelectedElections.Add(election);
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
            foreach (var election in allElections)
            {
                await LoadElectionData(election);
            }
            ElectionsLoaded = true;
        });
    }

    private async Task LoadElectionData(Election election, string tallyId = "")
    {
        ElectionGuardException.ThrowIfNull(election.ElectionId, $"Election did not have an electionId {election.Id}");

        var allUploads = await _ballotUploadService.GetByElectionIdAsync(election.ElectionId);
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
            var electionItem = new ElectionItem
            {
                Election = election,
                TallyId = tallyId,
                BallotUploads = new(allUploads),
                BallotAddedTotal = ballotAddedTotal,
                BallotChallengedTotal = ballotChallengedTotal,
                BallotDuplicateTotal = ballotDuplicateTotal,
                BallotRejectedTotal = ballotDuplicateTotal,
                BallotSpoiledTotal = ballotSpoiledTotal,
                BallotCountTotal = ballotCountTotal
            };
            Elections.Add(electionItem);
        }
    }

    private async Task FillKeyCeremonies()
    {
        var allKeys = await _keyCeremonyService.GetAllCompleteAsync();
        if (allKeys is not null)
        {
            foreach (var keyCeremony in allKeys)
            {
                var count = await _electionService.CountByKeyCeremonyIdAsync(keyCeremony.KeyCeremonyId);
                if (count > 1)
                {
                    KeyCeremonies.Add(keyCeremony);
                }
            }
        }
        else
        {
            await Shell.Current.CurrentPage.DisplayAlert(AppResources.CreateMultiTally, AppResources.CreateMultiTallyError, AppResources.OkText);
        }
    }

    [RelayCommand]
    private void SelectionChanged()
    {
        SelectAll = SelectedElections.Count != Elections.Count;
        CreateTalliesCommand.NotifyCanExecuteChanged();
        JoinTalliesCommand.NotifyCanExecuteChanged();
    }

    private async Task<(string, string)> CreateNewTally(ElectionItem electionItem, bool multi=false)
    {
        ElectionGuardException.ThrowIfNull(electionItem, $"Could not load election selected");
        ElectionGuardException.ThrowIfNull(electionItem.Election, $"ElectionItem does not contain an election");
        ElectionGuardException.ThrowIfNull(electionItem.Election.ElectionId, $"Election does not have an election id");
        ElectionGuardException.ThrowIfNull(electionItem.Election.KeyCeremonyId, $"Election does not have a key ceremony id");

        // calculate ballot count and upload count
        var ballotCount = await _ballotService.GetCountBallotsByElectionIdStateAsync(electionItem.Election.ElectionId, BallotBoxState.Cast);
        var challengedCount = await _ballotService.GetCountBallotsByElectionIdStateAsync(electionItem.Election.ElectionId, BallotBoxState.Challenged);
        var spoiledCount = await _ballotService.GetCountBallotsByElectionIdStateAsync(electionItem.Election.ElectionId, BallotBoxState.Spoiled);

        var keyCeremony = await _keyCeremonyService.GetByKeyCeremonyIdAsync(electionItem.Election.KeyCeremonyId);
        ElectionGuardException.ThrowIfNull(keyCeremony, $"Could not load key ceremony {electionItem.Election.KeyCeremonyId}");

        var tallyName = $"{electionItem.Election.Name} {AppResources.TallyText}";

        TallyRecord newTally = new()
        {
            Name = tallyName,
            ElectionId = electionItem.Election.ElectionId,
            KeyCeremonyId = electionItem.Election.KeyCeremonyId,
            Quorum = keyCeremony.Quorum,
            NumberOfGuardians = keyCeremony.NumberOfGuardians,
            CastBallotCount = ballotCount,
            ChallengedBallotCount = challengedCount,
            SpoiledBallotCount = spoiledCount,
            State = TallyState.PendingGuardiansJoin,
            MultiTally = multi,
        };

        _ = await _tallyService.SaveAsync(newTally);

        return (newTally.TallyId, newTally.Name);
    }


    [RelayCommand(CanExecute = nameof(TalliesSelected))]
    private async Task CreateTallies()
    {
        if(SelectedElections.Count == 1)
        {
            // do a normal tally for a single election
            var election = SelectedElections.First() as ElectionItem;

            var (tallyId, _) = await CreateNewTally(election);

            // go to the processing page
            await NavigationService.GoToPage(typeof(TallyProcessViewModel), new()
            {
                { TallyProcessViewModel.CurrentTallyIdParam, tallyId }
            });
        }
        else
        {
            List<(string, string, string)> tallys = new();
            // create tally records for each election selected
            foreach (var electionItem in SelectedElections)
            {
                var election = electionItem as ElectionItem;
                var (tallyId, electionName) = await CreateNewTally(election, true);
                tallys.Add((tallyId, election!.Election!.ElectionId!, electionName));
            }

            // create a multi-tally record for the db
            MultiTallyRecord multiTallyRecord = new()
            {
                Name = $"MultiTally for {SelectedKeyCeremony.Name}",
                KeyCeremonyId = SelectedKeyCeremony.KeyCeremonyId,
                TallyIds = tallys,
                ResultsPath = CurrentResultsPath
            };

            _ = await _multiTallyService.SaveAsync(multiTallyRecord);

            // go to the processing page
            await NavigationService.GoToPage(typeof(TallyProcessViewModel), new()
            {
                { TallyProcessViewModel.MultiTallyIdParam, multiTallyRecord.MultiTallyId }
            });
        }
    }

    private async Task JoinTally(string tallyId, bool join = true)
    {
        // join the tally
        var joiner = new TallyJoinedRecord()
        {
            TallyId = tallyId,
            GuardianId = UserName!, // can assume not null, since you need to be signed into 
            Joined = join,
        };

        await _tallyJoinedService.JoinTallyAsync(joiner);
    }

    [RelayCommand(CanExecute = nameof(TalliesSelected))]
    private async Task JoinTallies()
    {
        List<string> tallys = new();
        foreach (var election in Elections)
        {
            if (SelectedElections.Contains(election))
            {
                // join the tally
                await JoinTally(election.TallyId);
                tallys.Add(election.TallyId);
            }
            else
            {
                // reject the tally
                await JoinTally(election.TallyId, false);
            }
        }

        // do a normal tally for a single election but reject all of the others
        if (SelectedElections.Count == 1)
        {
            var election = SelectedElections.First() as ElectionItem;
            ElectionGuardException.ThrowIfNull(election, $"Could not load election selected");

            // go to the processing page
            await NavigationService.GoToPage(typeof(TallyProcessViewModel), new()
            {
                { TallyProcessViewModel.CurrentTallyIdParam, election.TallyId }
            });
        }
        else
        {
            // go to the processing page
            await NavigationService.GoToPage(typeof(TallyProcessViewModel), new()
            {
                { TallyProcessViewModel.MultiTallyIdParam, MultiTallyId }
            });
        }
    }

    private bool TalliesSelected() => SelectedElections.Count > 0 && !string.IsNullOrEmpty(CurrentResultsPath);

    [RelayCommand]
    private async Task PickFolder()
    {
        CancellationToken token = new();
        try
        {
            var folder = await FolderPicker.Default.PickAsync(token);
            CurrentResultsPath = folder.Folder?.Path;
        }
        catch (Exception ex)
        {
            CurrentResultsPath = string.Empty;
        }
    }
}

