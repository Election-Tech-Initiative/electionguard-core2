using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectionGuard.UI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isAdmin;

    [RelayCommand]
    void Spanish()
    {
        LocalizationResourceManager.Current.CurrentCulture = new CultureInfo("es");
    }

    [RelayCommand]
    void English()
    {
        LocalizationResourceManager.Current.CurrentCulture = new CultureInfo("en");
    }

}
