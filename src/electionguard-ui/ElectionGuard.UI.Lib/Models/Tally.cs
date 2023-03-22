using CommunityToolkit.Mvvm.ComponentModel;

namespace ElectionGuard.UI.Lib.Models;

public partial class Tally : DatabaseRecord
{
    [ObservableProperty]
    private string? _name;

    [ObservableProperty]
    private string? _electionId;

    [ObservableProperty]
    private string? _electionName;

    [ObservableProperty]
    private string? _keyCermonyId;

    [ObservableProperty]
    private int _quorum;

    [ObservableProperty]
    private int _numberOfGuardians;

    [ObservableProperty]
    private long _ballotCount;

    [ObservableProperty]
    private long _ballotUploadCount;

    [ObservableProperty]
    private DateTime _tallyStart;

    [ObservableProperty]
    private DateTime _tallyEnd;

    [ObservableProperty]
    private List<long> _deviceIds = new();

    [ObservableProperty]
    private DateTime _createdAt;


    public Tally() : base(nameof(Tally))
    {
    }

}

