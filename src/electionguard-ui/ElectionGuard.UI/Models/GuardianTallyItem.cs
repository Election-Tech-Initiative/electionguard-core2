using System.ComponentModel.DataAnnotations;

namespace ElectionGuard.UI.Models;

public partial class GuardianTallyItem : ObservableObject, IEquatable<GuardianTallyItem>
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private bool _hasDecryptShares;

    [ObservableProperty]
    private bool _hasResponse;

    [ObservableProperty]
    private bool _isSelf;

    [ObservableProperty]
    private bool _joined;

    public bool Equals(GuardianTallyItem? other)
    {
        if (other is null)
            return false;

        return Name == other.Name;
    }
}
