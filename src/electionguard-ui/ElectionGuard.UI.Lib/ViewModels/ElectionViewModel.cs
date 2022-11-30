﻿using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.Lib.ViewModels;

public partial class ElectionViewModel : BaseViewModel, IDisposable
{
    public ElectionViewModel(
        ILocalizationService localizationService,
        INavigationService navigationService,
        IConfigurationService configurationService) : base(localizationService, navigationService, configurationService)
    {
        PropertyChanged += ElectionViewModel_PropertyChanged;
    }

    private void ElectionViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CurrentElection))
        {
            PageTitle = CurrentElection?.Name ?? "";
        }
    }

    [ObservableProperty]
    private Election? _currentElection;

    public const string CurrentElectionParam = "CurrentElection";

    public void Dispose()
    {
        PropertyChanged -= ElectionViewModel_PropertyChanged;
    }
}
