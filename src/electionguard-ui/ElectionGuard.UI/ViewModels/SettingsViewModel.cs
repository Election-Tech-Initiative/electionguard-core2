using CommunityToolkit.Mvvm.Input;

namespace ElectionGuard.UI.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    public SettingsViewModel(IServiceProvider serviceProvider) : base(null, serviceProvider)
    {
    }

    [ObservableProperty]
    private string _databaseAddress = DbContext.DbHost;

    [ObservableProperty]
    private string _databasePassword = DbContext.DbPassword;

    [ObservableProperty]
    private string _databaseConnectionString = DbContext.DbConnection;

    partial void OnDatabaseConnectionStringChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(DatabaseConnectionString))
        {
            DatabaseAddress = string.Empty;
            DatabasePassword = string.Empty;
        }
    }

    [RelayCommand]
    private void Save()
    {
        // save db settings and reset the db connection
        DbContext.DbHost = DatabaseAddress;
        DbContext.DbPassword = DatabasePassword;
        DbContext.DbConnection = DatabaseConnectionString;

        if (!string.IsNullOrEmpty(DatabaseConnectionString))
        {
            DbService.Init(DatabaseConnectionString);
            return;
        }

        DbService.Init(DatabaseAddress, DatabasePassword);
    }

    [RelayCommand]
    private async Task Logs()
    {
        _ = await Launcher.Default.OpenAsync(ErrorLog.CreateLogPath());
    }


}
