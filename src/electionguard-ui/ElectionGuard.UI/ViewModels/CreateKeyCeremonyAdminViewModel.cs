using CommunityToolkit.Mvvm.Input;
using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.ViewModels;

public partial class CreateKeyCeremonyAdminViewModel : BaseViewModel
{
    private readonly KeyCeremonyService _keyCeremonyService;
    private const string PageName = "CreateKeyCeremony";

    public CreateKeyCeremonyAdminViewModel(IServiceProvider serviceProvider, KeyCeremonyService keyCeremonyService) : base(PageName, serviceProvider)
    {
        _keyCeremonyService = keyCeremonyService;
    }

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateKeyCeremonyCommand))]
    private string _keyCeremonyName = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateKeyCeremonyCommand))]
    private int _numberOfGuardians = 3;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateKeyCeremonyCommand))]
    private int _quorum = 3;

    [RelayCommand(CanExecute = nameof(CanCreate), AllowConcurrentExecutions = true)]
    public async Task CreateKeyCeremony()
    {
        var existingKeyCeremony = await _keyCeremonyService.GetByNameAsync(KeyCeremonyName);
        if (existingKeyCeremony != null)
        {
            var alreadyExists = LocalizationService.GetValue("AlreadyExists");
            ErrorMessage = $"{KeyCeremonyName} {alreadyExists}";
            CreateKeyCeremonyCommand.NotifyCanExecuteChanged();
            return;
        }

        var keyCeremony = new KeyCeremonyRecord(KeyCeremonyName, NumberOfGuardians, Quorum, this.UserName!);
        var ret = await _keyCeremonyService.SaveAsync(keyCeremony);
        var keyCeremonyId = ret.KeyCeremonyId!;
        await NavigationService.GoToPage(typeof(ViewKeyCeremonyViewModel), new Dictionary<string, object>
        {
            { ViewKeyCeremonyViewModel.CurrentKeyCeremonyParam, keyCeremonyId }
        });
    }

    private bool CanCreate()
    {
        return KeyCeremonyName.Trim().Length > 0 &&
            Quorum <= NumberOfGuardians;
    }
}

