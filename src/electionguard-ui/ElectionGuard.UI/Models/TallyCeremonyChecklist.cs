namespace ElectionGuard.UI.Models;

public partial class TallyCeremonyChecklist : ObservableObject
{
    public bool QuorumReached => State > TallyState.PendingGuardiansJoin;
    public bool TallyStarted => State == TallyState.TallyStarted;
    public bool SubtaliesCombined => State >= TallyState.PendingGuardianDecryptShares;
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

    public bool IsAbandoned => State == TallyState.Abandoned;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(QuorumReached))]
    [NotifyPropertyChangedFor(nameof(TallyStarted))]
    [NotifyPropertyChangedFor(nameof(SubtaliesCombined))]
    [NotifyPropertyChangedFor(nameof(AllDecryptionSharesComputed))]
    [NotifyPropertyChangedFor(nameof(ChallengeCreated))]
    [NotifyPropertyChangedFor(nameof(AllChallengesResponded))]
    [NotifyPropertyChangedFor(nameof(TallyComplete))]
    [NotifyPropertyChangedFor(nameof(IsAbandoned))]
    private TallyState _state;

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
