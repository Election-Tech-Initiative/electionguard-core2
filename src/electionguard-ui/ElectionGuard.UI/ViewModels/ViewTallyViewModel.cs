using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.Input;
using ElectionGuard.Decryption.Tally;
using ElectionGuard.UI.Lib.Models;
using ElectionGuard.UI.Models;
using Newtonsoft.Json;

namespace ElectionGuard.UI.ViewModels;

[QueryProperty(CurrentTallyIdParam, nameof(TallyId))]
public partial class ViewTallyViewModel : BaseViewModel
{
    public const string CurrentTallyIdParam = "TallyId";

    [ObservableProperty]
    private string _tallyId = string.Empty;

    [ObservableProperty]
    private TallyRecord? _tally = default;

    [ObservableProperty]
    private Election? _currentElection = default;

    [ObservableProperty]
    private Manifest? _originalManifest = default;

    [ObservableProperty]
    private InternalManifest? _currentManifest = default;

    [ObservableProperty]
    private PlaintextTally? _plaintextTally = default;

    [ObservableProperty]
    private ObservableCollection<GuardianTallyItem> _joinedGuardians = new();

    [ObservableProperty]
    private ObservableCollection<ContestItem> _contests = new();

    private readonly ElectionService _electionService;
    private readonly TallyService _tallyService;
    private readonly ManifestService _manifestService;
    private readonly BallotUploadService _ballotUploadService;
    private readonly TallyJoinedService _tallyJoinedService;
    private readonly DecryptionShareService _decryptionShareService;
    private readonly ChallengeResponseService _challengeResponseService;
    private readonly PlaintextTallyService _plaintextTallyService;

    public ViewTallyViewModel(
        IServiceProvider serviceProvider,
        TallyService tallyService,
        TallyJoinedService tallyJoinedService,
        ElectionService electionService,
        BallotUploadService ballotUploadService,
        DecryptionShareService decryptionShareService,
        PlaintextTallyService plaintextTallyService,
        ManifestService manifestService,
        ChallengeResponseService challengeResponseService,
        ILogger<ViewTallyViewModel> logger) :
        base("ViewTally", serviceProvider)
    {
        _tallyService = tallyService;
        _electionService = electionService;
        _manifestService = manifestService;
        _tallyJoinedService = tallyJoinedService;
        _ballotUploadService = ballotUploadService;
        _decryptionShareService = decryptionShareService;
        _challengeResponseService = challengeResponseService;
        _plaintextTallyService = plaintextTallyService;
        _logger = logger;
    }

    partial void OnTallyIdChanged(string value)
    {
        _ = Shell.Current.CurrentPage.Dispatcher.DispatchAsync(async () =>
        {
            Tally = await _tallyService.GetByTallyIdAsync(value);

            var plaintextRecord = await _plaintextTallyService.GetByTallyIdAsync(value);
            if (plaintextRecord is null)
            {
                throw new ElectionGuardException($"Plaintext tally not found! Tally {value}");
            }
            this.PlaintextTally = JsonConvert.DeserializeObject<PlaintextTally>(plaintextRecord);
        });
    }

    partial void OnTallyChanged(TallyRecord? oldValue, TallyRecord? newValue)
    {
        if (newValue is not null)
        {
            ArgumentException.ThrowIfNullOrEmpty(newValue.ElectionId);

            _ = Shell.Current.CurrentPage.Dispatcher.DispatchAsync(async () =>
            {
                var electionId = newValue.ElectionId ?? string.Empty;
                var election = await _electionService.GetByElectionIdAsync(electionId);
                ElectionGuardException.ThrowIfNull(election, $"ElectionId {electionId} not found! Tally {newValue.TallyId}");

                CurrentElection = election;

                await UpdateTallyData();
            });
        }
    }

    partial void OnCurrentElectionChanged(Election? oldValue, Election? newValue)
    {
        if (newValue is not null)
        {
            ArgumentException.ThrowIfNullOrEmpty(newValue.ElectionId);

            _ = Shell.Current.CurrentPage.Dispatcher.DispatchAsync(async () =>
            {
                var manifest = await _manifestService.GetByElectionIdAsync(newValue.ElectionId);
                ElectionGuardException.ThrowIfNull(manifest, $"Could not load manifest for election {newValue.ElectionId}");

                OriginalManifest = JsonConvert.DeserializeObject<Manifest>(manifest);
                CurrentManifest = new(OriginalManifest);

                GenerateContestData();
            });
        }
    }

    private async Task UpdateTallyData()
    {
        // if we have fewer than max number, see if anyone else joined
        if (JoinedGuardians.Count != Tally?.NumberOfGuardians)
        {
            var localData = await _tallyJoinedService.GetAllByTallyIdAsync(TallyId);

            foreach (var item in localData)
            {
                if (!JoinedGuardians.Any(g => g.Name == item.GuardianId))
                {
                    JoinedGuardians.Add(new GuardianTallyItem() { Name = item.GuardianId, Joined = item.Joined });
                }
            }
        }
        foreach (var guardian in JoinedGuardians)
        {
            guardian.HasDecryptShares = await _decryptionShareService.GetExistsByTallyAsync(TallyId, guardian.Name);
            guardian.HasResponse = await _challengeResponseService.GetExistsByTallyAsync(TallyId, guardian.Name);
            guardian.IsSelf = guardian.Name == UserName!;
        }
    }

    [RelayCommand]
    private async Task ExportTally()
    {
        var token = new CancellationToken();
        var outputResult = await FolderPicker.PickAsync(token);
        if (outputResult.IsSuccessful)
        {
            await ElectionRecordGenerator.GenerateElectionRecordAsync(Tally!, outputResult.Folder!.Path);
        }
    }

    private void GenerateContestData()
    {
        ElectionGuardException.ThrowIfNull(CurrentManifest);
        ElectionGuardException.ThrowIfNull(OriginalManifest);
        ElectionGuardException.ThrowIfNull(PlaintextTally);

        List<PartyDisplay> parties = new();
        List<CandidateDisplay> candidates = new();

        for (ulong i = 0; i < OriginalManifest.PartiesSize; i++)
        {
            var local = OriginalManifest.GetPartyAtIndex(i);
            parties.Add(new(local.Name.GetTextAt(0).Value, local.Abbreviation, local.ObjectId));
        }

        for (ulong i = 0; i < OriginalManifest.CandidatesSize; i++)
        {
            var local = OriginalManifest.GetCandidateAtIndex(i);
            var party = parties.FirstOrDefault(p => p.PartyId == local.PartyId) ?? new PartyDisplay(string.Empty, string.Empty, string.Empty); ;
            candidates.Add(new(local.Name.GetTextAt(0).Value, party.Name, local.ObjectId, local.IsWriteIn));
        }

        foreach (var (key, item) in PlaintextTally.Contests)
        {
            var contest = CurrentManifest.Contests.Single(c => c.ObjectId == item.ObjectId);
            var contestItem = new ContestItem() { Name = contest.Name, TotalVotes = contest.VotesAllowed * (ulong)Tally.CastBallotCount };
            ulong writeInTotal = 0;
            ulong totalVotes = 0;

            foreach (var (skey, selection) in item.Selections)
            {
                try
                {
                    var fullSelection = contest.Selections.Single(c => c.ObjectId == selection.ObjectId);
                    var candidate = candidates.Single(c => c.CandidateId == fullSelection.CandidateId);
                    var percent = (float)selection.Tally / (contest.VotesAllowed * (ulong)Tally.CastBallotCount) * 100;
                    if (!candidate.isWritein)
                    {
                        contestItem.Selections.Add(new() { Name = candidate.CandidateName, Party = candidate.Party, Votes = selection.Tally, Percent = percent });
                    }
                    else
                    {
                        writeInTotal += selection.Tally;
                    }
                    totalVotes += selection.Tally;
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error parsing contest {item.ObjectId} selection {selection.ObjectId}", item.ObjectId, selection.ObjectId);
                }
            }
            if (writeInTotal > 0)
            {
                var percent = (float)writeInTotal / (contest.VotesAllowed * (ulong)Tally.CastBallotCount) * 100;
                contestItem.Selections.Add(new() { Name = "Write-ins", Party = string.Empty, Votes = writeInTotal, Percent = percent });
            }

            // TallyOverVoteUndervote(contest, Tally, contestItem, totalVotes);

            Contests.Add(contestItem);
        }
    }

    // TODO: Fix calculation and add to contest item.
    private void TallyOverVoteUndervote(ContestDescriptionWithPlaceholders contest, ContestItem contestItem, ulong totalVotes)
    {
        if (Tally == null)
        {
            return;
        }

        var overVoteTotal = 0UL;
        var underVoteTotal = (contest.VotesAllowed * (ulong)Tally.CastBallotCount) - totalVotes;
        var underPercent = (float)underVoteTotal / (contest.VotesAllowed * (ulong)Tally.CastBallotCount) * 100;

        contestItem.Selections.Add(new() { Name = "Undervotes", Party = string.Empty, Votes = underVoteTotal, Percent = underPercent });
        var overPercent = (float)overVoteTotal / (contest.VotesAllowed * (ulong)Tally.CastBallotCount) * 100;

        contestItem.Selections.Add(new() { Name = "Overvotes", Party = string.Empty, Votes = overVoteTotal, Percent = overPercent });
    }

    public override async Task OnLeavingPage()
    {
        OriginalManifest?.Dispose();
        CurrentManifest?.Dispose();
        PlaintextTally?.Dispose();

        await base.OnLeavingPage();
    }
}
