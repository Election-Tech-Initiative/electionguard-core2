using System.Data.Common;
using System.Runtime.InteropServices.JavaScript;
using CommunityToolkit.Mvvm.Input;

namespace ElectionGuard.UI.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    public SettingsViewModel(IServiceProvider serviceProvider) : base(null, serviceProvider)
    {
    }

    [ObservableProperty]
    private string _databaseAddress = Preferences.Get("DbAddress", "127.0.0.1");

    [ObservableProperty]
    private string _databasePassword = Preferences.Get("DbPassword", string.Empty);

    [ObservableProperty]
    private string _databaseConnectionString = Preferences.Get("DbConnection", string.Empty);

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
        Preferences.Set("DbAddress", DatabaseAddress);
        Preferences.Set("DbPassword", DatabasePassword);
        Preferences.Set("DbConnection", DatabaseConnectionString);

        if (!string.IsNullOrEmpty(DatabaseConnectionString))
        {
            DbService.Init(DatabaseConnectionString);
        }
        else
        {
            DbService.Init(DatabaseAddress, DatabasePassword);
        }
    }

}
