using CommunityToolkit.Mvvm.ComponentModel;

namespace ElectionGuard.UI.Lib.Models;

public partial class Tally : DatabaseRecord
{
    [ObservableProperty]
    private string? _name;

    [ObservableProperty]
    private DateTime _createdAt;


    public Tally() : base(nameof(Tally))
    {

    }

}

