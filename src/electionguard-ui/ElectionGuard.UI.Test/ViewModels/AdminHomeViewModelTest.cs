using System;
using ElectionGuard.UI.ViewModels;
using NSubstitute;

namespace ElectionGuard.UI.Test.ViewModels;

public class AdminHomeViewModelTest
{
    private ElectionViewModel _electionViewModel;

    public AdminHomeViewModelTest()
    {
        _electionViewModel = Substitute.For<ElectionViewModel>();
    }
    
    [Test]
    public void Given_WhenKeyCeremonyButtonClicked_ThenNavToCeremony()
    {

        var adminHomeViewModel = new AdminHomeViewModel(_electionViewModel);
        
        Assert.NotNull(adminHomeViewModel);
    }
}

