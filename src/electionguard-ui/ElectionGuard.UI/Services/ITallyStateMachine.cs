namespace ElectionGuard.UI.Services;

public interface ITallyStateMachine
{
    TallyState CurrentState { get; }
    string TallyId { get; set; }
    string ElectionId { get; set; }
    Task Run();
}
