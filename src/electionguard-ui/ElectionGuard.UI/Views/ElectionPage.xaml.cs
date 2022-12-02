namespace ElectionGuard.UI.Views;

public partial class ElectionPage : IQueryAttributable
{
    public ElectionPage(ElectionViewModel electionViewModel) : base(electionViewModel)
    {
        InitializeComponent();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        var currentElection = query[ElectionViewModel.CurrentElectionParam] as Election;
        ViewModel.CurrentElection = currentElection;
    }
}