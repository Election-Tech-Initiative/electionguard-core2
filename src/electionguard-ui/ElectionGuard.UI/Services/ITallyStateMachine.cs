namespace ElectionGuard.UI.Services;

public interface ITallyStateMachine
{
    Task<bool> Run(TallyRecord tally);
}
