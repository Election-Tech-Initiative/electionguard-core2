namespace ElectionGuard.UI.Controls;

public partial class ContestControl : ContentView
{
    public ContestControl()
    {
        InitializeComponent();
        BindingContext = this;
        GenerateSelections();
    }

    private void GenerateSelections()
    {

    }

    public List<TallyItem> Selections { get; set; } = new();

    public string Name { get; set; } = string.Empty;

    public static readonly BindableProperty ContestProperty = BindableProperty.Create(nameof(Contest), typeof(PlaintextBallotContest), typeof(ContestControl));

    public PlaintextBallotContest Contest
    {
        get => (PlaintextBallotContest)GetValue(ContestProperty);
        set => SetValue(ContestProperty, value);
    }

    public static readonly BindableProperty ManifestProperty = BindableProperty.Create(nameof(Manifest), typeof(Manifest), typeof(ContestControl));

    public Manifest Manifest
    {
        get => (Manifest)GetValue(ManifestProperty);
        set => SetValue(ManifestProperty, value);
    }

    public static readonly BindableProperty TotalVotesProperty = BindableProperty.Create(nameof(TotalVotes), typeof(ulong), typeof(ContestControl));

    public ulong TotalVotes
    {
        get => (ulong)GetValue(TotalVotesProperty);
        set => SetValue(TotalVotesProperty, value);
    }

}
