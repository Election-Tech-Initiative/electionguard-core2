namespace ElectionGuard.UI.Views;

public partial class ElectionPage : IQueryAttributable
{
    private readonly ElectionViewModel _electionViewModel;

    public ElectionPage(ElectionViewModel electionViewModel)
    {
        _electionViewModel = electionViewModel;
        InitializeComponent();
		BindingContext = electionViewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        var currentElection = query[ElectionViewModel.CurrentElectionParam] as Election;
        _electionViewModel.CurrentElection = currentElection;
    }
}