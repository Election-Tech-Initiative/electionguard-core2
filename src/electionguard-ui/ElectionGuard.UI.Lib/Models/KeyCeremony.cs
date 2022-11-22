using CommunityToolkit.Mvvm.ComponentModel;

namespace ElectionGuard.UI.Lib.Models;

public partial class KeyCeremony : ObservableObject
{
    [ObservableProperty]
    private string? name;
}
