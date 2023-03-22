namespace ElectionGuard.Decryption.Tally;

public class TallyMediator : DisposableBase
{
    // TODO: proper data storage, such as in the mongo database
    public Dictionary<string, CiphertextTally> Tallies { get; init; } = new Dictionary<string, CiphertextTally>();

    public TallyMediator()
    {

    }

    public CiphertextTally CreateTally(
        string name,
        CiphertextElectionContext context,
        InternalManifest manifest)
    {
        var tally = new CiphertextTally(name, context, manifest);
        Tallies.Add(tally.TallyId, tally);
        return tally;
    }

    public CiphertextTally CreateTally(
        string tallyId,
        string name,
        CiphertextElectionContext context,
        InternalManifest manifest)
    {
        var tally = new CiphertextTally(tallyId, name, context, manifest);
        Tallies.Add(tally.TallyId, tally);
        return tally;
    }

    public CiphertextTally GetTally(string tallyId)
    {
        return Tallies[tallyId];
    }
}
