using System.Collections.ObjectModel;

namespace ElectionGuard.UI.Lib.Models;

public partial class ContestItem : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private ObservableCollection<TallyItem> _selections = new();

    [ObservableProperty]
    private ulong _totalVotes = 0;
}
