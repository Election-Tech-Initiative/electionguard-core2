using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.Input;
using ElectionGuard.Decryption.Tally;
using ElectionGuard.Encryption.Utils.Converters;
using ElectionGuard.UI.Lib.Models;
using ElectionGuard.UI.Models;
using Newtonsoft.Json;
using static MongoDB.Driver.WriteConcern;

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
    private PlaintextTally? _plaintextTally = default;

    [ObservableProperty]
    private ObservableCollection<GuardianTallyItem> _joinedGuardians = new();

    private readonly ElectionService _electionService;
    private readonly TallyService _tallyService;
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
        ChallengeResponseService challengeResponseService) :
        base("ViewTally", serviceProvider)
    {
        _tallyService = tallyService;
        _electionService = electionService;
        _tallyJoinedService = tallyJoinedService;
        _ballotUploadService = ballotUploadService;
        _decryptionShareService = decryptionShareService;
        _challengeResponseService = challengeResponseService;
        _plaintextTallyService = plaintextTallyService;
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
            try
            {
                this.PlaintextTally = JsonConvert.DeserializeObject<PlaintextTally>(plaintextRecord, SerializationSettings.NewtonsoftSettings());

            }
            catch (Exception ex)
            {
                throw;
            }
        });
    }

    partial void OnTallyChanged(TallyRecord? oldValue, TallyRecord? newValue)
    {
        if (newValue is not null && oldValue?.TallyId != newValue?.TallyId)
        {
            _ = Shell.Current.CurrentPage.Dispatcher.DispatchAsync(async () =>
            {
                var electionId = newValue.ElectionId ?? string.Empty;
                var election = await _electionService.GetByElectionIdAsync(electionId);
                if (election is null)
                {
                    throw new ElectionGuardException($"ElectionId {electionId} not found! Tally {newValue.TallyId}");
                    // TODO: put up some error message somewhere, over the rainbow.
                }
                CurrentElection = election;

                await UpdateTallyData();
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
                    JoinedGuardians.Add(new GuardianTallyItem() { Name = item.GuardianId });
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
        CancellationToken token = new CancellationToken();
        var outputResult = await FolderPicker.PickAsync(token);
        if (outputResult.IsSuccessful)
        {
            await ElectionRecordGenerator.GenerateElectionRecordAsync(Tally!, outputResult.Folder!.Path);
        }
    }

}
