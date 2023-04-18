﻿using CommunityToolkit.Mvvm.Input;
using ElectionGuard.UI.Lib.Services;
using MongoDB.Driver.Core.Clusters;

namespace ElectionGuard.UI.ViewModels;

public partial class CreateMultiTallyViewModel : BaseViewModel
{
    private TallyService _tallyService;
    private ElectionService _electionService;
    private ManifestService _manifestService;
    private KeyCeremonyService _keyCeremonyService;
    private BallotUploadService _ballotUploadService;

    public CreateMultiTallyViewModel(IServiceProvider serviceProvider, TallyService tallyService, ManifestService manifestService, ElectionService electionService, KeyCeremonyService keyCeremonyService, BallotUploadService ballotUploadService) : base("CreateMultiTally", serviceProvider)
    {
        _tallyService = tallyService;
        _manifestService = manifestService;
        _electionService = electionService;
        _keyCeremonyService = keyCeremonyService;
        _ballotUploadService = ballotUploadService;
        _ = Task.Run(FillKeyCeremonies);
    }

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private List<KeyCeremonyRecord> _keyCeremonies = new();

    [ObservableProperty]
    private KeyCeremonyRecord _selectedKeyCeremony;

    [ObservableProperty]
    private List<ElectionItem> _elections = new();

    partial void OnSelectedKeyCeremonyChanged(KeyCeremonyRecord value)
    {
        // fill in the list of the elections that use the current key ceremony
        _ = Task.Run(async () =>
        {
            var allElections = await _electionService.GetAllByKeyCeremonyIdAsync(value.KeyCeremonyId);
            foreach (var item in allElections)
            {
                var allUploads = await _ballotUploadService.GetByElectionIdAsync(item.ElectionId);
                var ballotCountTotal = 0L;
                var ballotAddedTotal = 0L;
                var ballotSpoiledTotal = 0L;
                var ballotDuplicateTotal = 0L;
                var ballotRejectedTotal = 0L;

                allUploads.ForEach((upload) =>
                {
                    ballotCountTotal += upload.BallotCount;
                    ballotAddedTotal += upload.BallotImported;
                    ballotSpoiledTotal += upload.BallotSpoiled;
                    ballotDuplicateTotal += upload.BallotDuplicated;
                    ballotRejectedTotal += upload.BallotRejected;
                });
                var election = new ElectionItem { 
                    Election = item,
                    BallotUploads = allUploads,
                    BallotAddedTotal = ballotAddedTotal,
                    BallotDuplicateTotal = ballotDuplicateTotal,
                    BallotRejectedTotal = ballotDuplicateTotal,
                    BallotSpoiledTotal = ballotSpoiledTotal,
                    BallotCountTotal = ballotCountTotal };
                Elections.Add(election);
            }
        });
    }

    private async Task FillKeyCeremonies()
    {
//        var allKeys = await _keyCeremonyService.GetAllCompleteAsync();
        var allKeys = await _keyCeremonyService.GetAllAsync();
        foreach (var item in allKeys)
        {
            var count = await _electionService.CountByKeyCeremonyIdAsync(item.KeyCeremonyId);
            if(count >= 0)
            {
                KeyCeremonies.Add(item);
            }
        }
    }


    [RelayCommand]
    private void CreateTallies()
    {

    }

    [RelayCommand]
    private void JoinTallies()
    {

    }
}

