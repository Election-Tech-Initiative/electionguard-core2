namespace ElectionGuard.UI.Models;

public partial class TallyCeremonyChecklist : ObservableObject
{
    [ObservableProperty]
    private bool _quorumReached;

    [ObservableProperty]
    private bool _tallyStarted;

    [ObservableProperty]
    private bool _subtaliesCombined;

    [ObservableProperty]
    private bool _allDecryptionSharesComputed;

    [ObservableProperty]
    private bool _challengeCreated;

    [ObservableProperty]
    private bool _allChallengesResponded;

    [ObservableProperty]
    private bool _tallyComplete;

    [ObservableProperty]
    private bool _isAbandoned;

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

    private readonly int _guardiansJoined;
    private readonly int _quorum;
    private readonly int _sharesComputed;
    private readonly int _challengesResponded;

    public TallyCeremonyChecklist() { }

    public TallyCeremonyChecklist(
        TallyRecord tally,
        int guardiansJoined,
        int sharesComputed,
        int challengesResponded)
    {
        _quorum = tally.Quorum;
        _guardiansJoined = guardiansJoined;
        _sharesComputed = sharesComputed;
        _challengesResponded = challengesResponded;
        QuorumReached = _guardiansJoined >= _quorum;
        State = tally.State;
        SubtaliesCombined = State >= TallyState.PendingGuardianDecryptShares;
        AllDecryptionSharesComputed = SubtaliesCombined &&
            (State > TallyState.PendingGuardianDecryptShares ||
            (State == TallyState.PendingGuardianDecryptShares && _sharesComputed >= _quorum));
        ChallengeCreated = AllDecryptionSharesComputed && State >= TallyState.PendingGuardianRespondChallenge;
        AllChallengesResponded = ChallengeCreated &&
            (State > TallyState.PendingGuardianRespondChallenge ||
            (State == TallyState.PendingGuardianRespondChallenge && _challengesResponded == _quorum));
        TallyComplete = State == TallyState.Complete;
        TallyStarted = State >= TallyState.TallyStarted;
        IsAbandoned = State == TallyState.Abandoned;
    }
}
