using System.Text;

namespace ElectionGuard.UI.ViewModels;

public record PartyDisplay(string Name, string Abbreviation, string PartyId);
public record CandidateDisplay(string CandidateName, string Party, string CandidateId, bool isWritein=false);
public record BallotStyleDisplay(string Name, string Units);
public record GeopoliticalUnitDisplay(string UnitName, string GPType, string GeopoliticalUnitId);
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
    private string _manifestName = string.Empty;

    [ObservableProperty]
    private string _Name = string.Empty;

    [ObservableProperty]
    private string _units = string.Empty;

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

    partial void OnManifestChanged(Manifest? value)
    {
        Parties.Clear();
        Candidates.Clear();
        GeopoliticalUnits.Clear();
        BallotStyles.Clear();
        Contests.Clear();

        if (value == null)
        {
            return;
        }

        ManifestName = value.Name.GetTextAt(0).Value;

        for (ulong i = 0; i < value.PartiesSize; i++)
        {
            var local = value.GetPartyAtIndex(i);
            Parties.Add(new(local.Name.GetTextAt(0).Value, local.Abbreviation, local.ObjectId));
        }

        for (ulong i = 0; i < value.CandidatesSize; i++)
        {
            var local = Manifest.GetCandidateAtIndex(i);
            var party = Parties.FirstOrDefault(p => p.PartyId == local.PartyId) ?? new PartyDisplay(string.Empty, string.Empty, string.Empty);
            var name = local.Name.GetTextAt(0).Value;
            if(name == string.Empty)
            {
                name = AppResources.WriteinText;
            }
            Candidates.Add(new(name, party.Name, local.ObjectId));
        }

        for (ulong i = 0; i < value.GeopoliticalUnitsSize; i++)
        {
            var local = value.GetGeopoliticalUnitAtIndex(i);
            GeopoliticalUnits.Add(new(local.Name, local.ReportingUnitType.ToString(), local.ObjectId));
        }

        for (ulong i = 0; i < value.BallotStylesSize; i++)
        {
            var local = value.GetBallotStyleAtIndex(i);
            var gpunits = new List<string>();
            var units = new StringBuilder();
            var geopoliticalUnits = local.GeopoliticalUnitIds;
            for (ulong j = 0; j < local.GeopoliticalUnitIdsSize; j++)
            {
                var unit = GeopoliticalUnits.FirstOrDefault(u => u.GeopoliticalUnitId == local.GetGeopoliticalUnitIdAtIndex(j)) ?? new(string.Empty, string.Empty, string.Empty);
                gpunits.Add(unit.UnitName);
                units.AppendLine(unit.UnitName);
            }
            BallotStyles.Add(new(local.ObjectId, units.ToString()));
        }

        for (ulong i = 0; i < value.ContestsSize; i++)
        {
            var local = value.GetContestAtIndex(i);
            var selections = new StringBuilder();
            for (ulong j = 0; j < local.SelectionsSize; j++)
            {
                var selection = local.GetSelectionAtIndex(j);
                var candidate = Candidates.FirstOrDefault(c => c.CandidateId == selection.CandidateId);
                if (candidate != null)
                {
                    selections.Append($"{candidate.CandidateName}");
                    if (!string.IsNullOrEmpty(candidate.Party))
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
