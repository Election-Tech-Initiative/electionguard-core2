namespace ElectionGuard.UI.Services;

public class TallyCeremonyChecklist
{
    public bool QuorumReached => State > TallyState.PendingGuardiansJoin;
    public bool SubtaliesCombined => QuorumReached && State >= TallyState.PendingGuardianDecryptShares;
    public bool AllDecryptionSharesComputed => SubtaliesCombined && (
        State > TallyState.PendingGuardianDecryptShares ||
        (State == TallyState.PendingGuardianDecryptShares && _sharesComputed >= _quorum));

    public bool ChallengeCreated =>
        AllDecryptionSharesComputed && State >= TallyState.PendingGuardianRespondChallenge;
    public bool AllChallengesResponded =>
        ChallengeCreated && (
        State > TallyState.PendingGuardianRespondChallenge ||
        (State == TallyState.PendingGuardianRespondChallenge && _challengesResponded == _quorum));
    public bool TallyComplete => State == TallyState.Complete;

    public TallyState State { get; set; }

    private readonly int _quorum;
    private readonly int _sharesComputed;
    private readonly int _challengesResponded;

    public TallyCeremonyChecklist() { }

    public TallyCeremonyChecklist(TallyRecord tally, int sharesComputed, int challengesResponded)
    {
        State = tally.State;
        _quorum = tally.Quorum;
        _sharesComputed = sharesComputed;
        _challengesResponded = challengesResponded;
    }
}
