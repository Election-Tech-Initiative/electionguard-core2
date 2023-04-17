namespace ElectionGuard.UI.Lib.Models;

public partial class ElectionItem : ObservableObject
{
    [ObservableProperty]
    private Election? _election;

    [ObservableProperty]
    private List<BallotUpload> _ballotUploads = new();

}
