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
    private string databaseAddress = Preferences.Get("DbAddress", "127.0.0.1");

    [ObservableProperty]
    private string databasePassword = Preferences.Get("DbPassword", "");

    [ObservableProperty]
    private string databaseConnectionString = Preferences.Get("DbConnection", "");

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
