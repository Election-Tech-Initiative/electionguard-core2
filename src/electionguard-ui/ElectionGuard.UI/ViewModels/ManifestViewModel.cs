using System.Text;

namespace ElectionGuard.UI.ViewModels;

public record PartyDisplay(string Name, string Abbreviation, string PartyId);
public record CandidateDisplay(string Name, string Party, string CandidateId);
public record BallotStyleDisplay(string Name, List<string> GeoPoliticalUnits);
public record GeopoliticalUnitDisplay(string Name, string GPType, string GeopoliticalUnitId);
public record ContestDisplay(string Name, string Variation, ulong NumberElected, ulong VotesAllowed, string Selections);


public partial class ManifestViewModel : BaseViewModel
{
    private ManifestService _manifestService;

    public ManifestViewModel(IServiceProvider serviceProvider, ManifestService manifestService) : base("ManifestViewer", serviceProvider)
    {
        _manifestService = manifestService;
    }

    [ObservableProperty]
    private Manifest? _manifest;

    [ObservableProperty]
    private string _manifestName;

    [ObservableProperty]
    private string _manifestFile = string.Empty;

    [ObservableProperty]
    private ObservableCollection<PartyDisplay> _parties = new();

    [ObservableProperty]
    private ObservableCollection<CandidateDisplay> _candidates = new();

    [ObservableProperty]
    private ObservableCollection<BallotStyleDisplay> _ballotStyles = new();

    [ObservableProperty]
    private ObservableCollection<GeopoliticalUnitDisplay> _geopoliticalUnits = new();

    [ObservableProperty]
    private ObservableCollection<ContestDisplay> _contests = new();


    partial void OnManifestFileChanged(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            Manifest = null;
            return;
        }

        this.Manifest = new Manifest(File.ReadAllText(value));
    }

    partial void OnManifestChanged(Manifest value)
    {
        Parties.Clear();
        Candidates.Clear();
        GeopoliticalUnits.Clear();
        BallotStyles.Clear();
        Contests.Clear();

        if (Manifest == null)
        {
            return;
        }

        ManifestName = Manifest.Name.GetTextAt(0).Value;

        for (ulong i = 0; i < Manifest.PartiesSize; i++)
        {
            var local = Manifest.GetPartyAtIndex(i);
            Parties.Add(new(local.Name.GetTextAt(0).Value, local.Abbreviation, local.ObjectId));
        }

        for (ulong i = 0; i < Manifest.CandidatesSize; i++)
        {
            var local = Manifest.GetCandidateAtIndex(i);
            var party = Parties.FirstOrDefault(p => p.PartyId == local.PartyId) ?? new PartyDisplay(string.Empty, string.Empty, string.Empty);
            var name = local.Name.GetTextAt(0).Value;
            if(name == string.Empty)
            {
                name = "Writein";
            }
            Candidates.Add(new(name, party.Name, local.ObjectId));
        }

        for (ulong i = 0; i < Manifest.GeopoliticalUnitsSize; i++)
        {
            var local = Manifest.GetGeopoliticalUnitAtIndex(i);
            GeopoliticalUnits.Add(new(local.Name, local.ReportingUnitType.ToString(), local.ObjectId));
        }

        for (ulong i = 0; i < Manifest.BallotStylesSize; i++)
        {
            var local = Manifest.GetBallotStyleAtIndex(i);
            var gpunits = new List<string>();
            var geopoliticalUnits = local.GeopoliticalUnitIds;
            for (ulong j = 0; j < local.GeopoliticalUnitIdsSize; j++)
            {
                var unit = GeopoliticalUnits.FirstOrDefault(u => u.GeopoliticalUnitId == local.GetGeopoliticalUnitIdAtIndex(j)) ?? new(string.Empty, string.Empty, string.Empty);
                gpunits.Add(unit.Name);
            }
            BallotStyles.Add(new(local.ObjectId, gpunits));
        }

        for (ulong i = 0; i < Manifest.ContestsSize; i++)
        {
            var local = Manifest.GetContestAtIndex(i);
            var selections = new StringBuilder();
            for (ulong j = 0; j < local.SelectionsSize; j++)
            {
                var selection = local.GetSelectionAtIndex(j);
                var candidate = Candidates.FirstOrDefault(c => c.CandidateId == selection.CandidateId);
                if (candidate != null)
                {
                    selections.Append($"{candidate.Name}");
                    if (string.IsNullOrEmpty(candidate.Party))
                    {
                        selections.Append($" ({candidate.Party})");
                    }
                    selections.AppendLine();
                }
            }
            Contests.Add(new ContestDisplay(local.Name, local.VoteVariationType.ToString(), local.NumberElected, local.VotesAllowed, selections.ToString()));
        }
    }

}
