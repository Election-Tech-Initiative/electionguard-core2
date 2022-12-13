namespace ElectionGuard.UI.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    public SettingsViewModel(IServiceProvider serviceProvider) : base(null, serviceProvider)
    {
    }

    [ObservableProperty]
    private string databaseAddress = Preferences.Get("DbAddress", "127.0.0.1");

    [ObservableProperty]
    private string databasePassword = Preferences.Get("DbPassword", "");

    [RelayCommand]
    private void Save()
    {
        // save db settings and reset the db connection
        Preferences.Set("DbAddress", DatabaseAddress);
        Preferences.Set("DbPassword", DatabasePassword);
        DbService.Init(DatabaseAddress, DatabasePassword);
    }

}
