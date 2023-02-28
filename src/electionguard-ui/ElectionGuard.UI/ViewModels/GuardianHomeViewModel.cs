﻿using CommunityToolkit.Mvvm.Input;
using ElectionGuard.UI.Lib.Models;
using ElectionGuard.UI.Lib.Services;

namespace ElectionGuard.UI.ViewModels;

public partial class GuardianHomeViewModel : BaseViewModel
{
    private readonly KeyCeremonyService _keyCeremonyService;

    public GuardianHomeViewModel(IServiceProvider serviceProvider, KeyCeremonyService keyCeremonyService) : base("GuardianHome", serviceProvider)
    {
        _keyCeremonyService = keyCeremonyService;
        // create some fake tallies to add to the list
        _tallies.Add(new Tally { Name = "Election Test Tally #1" });
        _tallies.Add(new Tally { Name = "Election Test Tally #2" });
        _tallies.Add(new Tally { Name = "Real Election Tally" });

        _timer = Dispatcher.GetForCurrentThread()!.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(UISettings.LONG_POLLING_INTERVAL);
        _timer.IsRepeating = true;
        _timer.Tick += PollingTimer_Tick;

    }

    public override async Task OnAppearing()
    {
        //_timer.Start();
        PollingTimer_Tick(this, null);
    }

    [ObservableProperty]
    private ObservableCollection<KeyCeremony> _keyCeremonies = new();

    [ObservableProperty]
    private ObservableCollection<Tally> _tallies = new();

    [ObservableProperty]
    private KeyCeremony? _currentKeyCeremony;

    [ObservableProperty]
    private Tally? _currentTally;

    private readonly IDispatcherTimer _timer;

    public override async Task OnLeavingPage()
    {
        _timer.Stop();
        await Task.Yield();
    }

    partial void OnCurrentKeyCeremonyChanged(KeyCeremony? value)
    {
        if (CurrentKeyCeremony == null) return;
        MainThread.BeginInvokeOnMainThread(async() =>
            await NavigationService.GoToPage(typeof(ViewKeyCeremonyViewModel), new Dictionary<string, object>
            {
                { "KeyCeremonyId", CurrentKeyCeremony.KeyCeremonyId! }
            }));
    }

    private async void PollingTimer_Tick(object? sender, EventArgs e)
    {
        var keyCeremonies = await _keyCeremonyService.GetAllNotCompleteAsync();
        KeyCeremonies.Clear();
        foreach (var item in keyCeremonies)
        {
            KeyCeremonies.Add(item);
        }
    }


}
