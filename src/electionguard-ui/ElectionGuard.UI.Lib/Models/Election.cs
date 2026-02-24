namespace ElectionGuard.UI.Lib.Models;

public partial class Election : DatabaseRecord
{
    [ObservableProperty]
    private string? _electionId;

    [ObservableProperty]
    private string? _name;

    [ObservableProperty]
    private string? _electionUrl;

    [ObservableProperty]
    private string? _keyCeremonyId;

    [ObservableProperty]
    private DateTime? _exportEncryptionDateTime;

    [ObservableProperty]
    private string? _createdBy;

    [ObservableProperty]
    private DateTime _createdAt;

    public Election() : base(nameof(Election))
    {
        ElectionId = Guid.NewGuid().ToString();
        CreatedAt = DateTime.UtcNow;
    }

}
