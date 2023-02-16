using CommunityToolkit.Mvvm.Input;
using ElectionGuard.ElectionSetup;

namespace ElectionGuard.UI.ViewModels;

[QueryProperty(CurrentKeyCeremonyParam, "KeyCeremonyId")]
public partial class ViewKeyCeremonyViewModel : BaseViewModel
{
    public const string CurrentKeyCeremonyParam = "KeyCeremonyId";

    private KeyCeremonyMediator? _mediator;

    public ViewKeyCeremonyViewModel(IServiceProvider serviceProvider, KeyCeremonyService keyCeremonyService) : 
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
    private string _keyCeremonyId = string.Empty;

    partial void OnKeyCeremonyIdChanged(string value)
    {
        Task.Run(async() => KeyCeremony = await _keyCeremonyService.GetByKeyCeremonyIdAsync(value));
    }

    partial void OnKeyCeremonyChanged(KeyCeremony? value)
    {
        if (value is not null)
        {
            _mediator = new KeyCeremonyMediator("mediator", UserName!, value);
            Task.Run(async () => await _mediator.RunKeyCeremony(IsAdmin));
        }
    }

    [RelayCommand(CanExecute = nameof(CanJoin))]
    public void Join()
    {
        var currentGuardianUserName = AuthenticationService.UserName;
    }

    private bool CanJoin()
    {
        return KeyCeremony is not null;
    }

    private readonly KeyCeremonyService _keyCeremonyService;
}
