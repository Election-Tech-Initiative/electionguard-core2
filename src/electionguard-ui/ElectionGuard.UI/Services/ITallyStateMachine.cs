namespace ElectionGuard.UI.Services;

public interface ITallyStateMachine
{
    Task Run(TallyRecord tally);
}
