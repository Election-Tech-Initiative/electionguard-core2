using ElectionGuard.ElectionSetup;
using ElectionGuard.ElectionSetup.Extensions;
using ElectionGuard.Extensions;

namespace ElectionGuard.Decryption.Shares;


public record DecryptionShare : DisposableRecordBase
{
public TallyShare TallyShare { get; set; }
public Dictionary<string, BallotShare> BallotShares { get; set; }

public DecryptionShare(TallyShare tallyShare, Dictionary<string, BallotShare> ballotShares) : base()
{
    TallyShare = tallyShare;
    BallotShares = ballotShares;
}

protected override void DisposeManaged()
{
    base.DisposeManaged();
    TallyShare.Dispose();
    BallotShares.Dispose();
}
}
