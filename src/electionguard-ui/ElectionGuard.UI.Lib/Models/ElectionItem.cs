using System.Collections.ObjectModel;

namespace ElectionGuard.UI.Lib.Models;

public partial class ElectionItem : ObservableObject
{
    [ObservableProperty]
    private Election? _election;

    [ObservableProperty]
    private string _tallyId = string.Empty;

    [ObservableProperty]
    private ObservableCollection<BallotUpload> _ballotUploads = new();

    [ObservableProperty]
    private long _ballotAddedTotal;

    [ObservableProperty]
    private long _ballotChallengedTotal;

    [ObservableProperty]
    private long _ballotSpoiledTotal;

    [ObservableProperty]
    private long _ballotDuplicateTotal;

    [ObservableProperty]
    private long _ballotRejectedTotal;

    [ObservableProperty]
    private long _ballotCountTotal;
}
