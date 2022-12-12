using CommunityToolkit.Mvvm.Input;
using ElectionGuard.ElectionSetup;
using ElectionGuard.UI.Lib.Services;

namespace ElectionGuard.UI.ViewModels;

[QueryProperty(CurrentKeyCeremonyParam, "KeyCeremonyId")]
public partial class ViewKeyCeremonyViewModel : BaseViewModel
{
    public const string CurrentKeyCeremonyParam = "KeyCeremonyId";

    public ViewKeyCeremonyViewModel(IServiceProvider serviceProvider, IKeyCeremonyService keyCeremonyService) : 
        base("ViewKeyCeremony", serviceProvider)
    {
        _keyCeremonyService = keyCeremonyService;
        IsJoinVisible = !AuthenticationService.IsAdmin;
    }

    [ObservableProperty]
    private KeyCeremony? _keyCeremony;

    [ObservableProperty] 
    private bool _isJoinVisible;

    [ObservableProperty] 
    private int _keyCeremonyId;

    public override async Task Appearing()
    {
        KeyCeremony = await _keyCeremonyService.Get(KeyCeremonyId);
    }

    [RelayCommand]
    public void Join()
    {
        if (KeyCeremony == null) throw new ArgumentNullException(nameof(KeyCeremony));
        var currentGuardianUserName = AuthenticationService.UserName;
        var guardian = Guardian.FromNonce(currentGuardianUserName, 0, KeyCeremony.NumberOfGuardians, KeyCeremony.Quorum);
    }

    private readonly IKeyCeremonyService _keyCeremonyService;
}
