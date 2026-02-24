namespace ElectionGuard.ElectionSetup;

/// <summary>
/// Data structure for keeping track of the steps needed for admin/guardians
/// </summary>
public class StateMachineStep<T> where T : struct, IConvertible
{
    /// <summary>
    /// The state of the KeyCeremony that this step needs to be run on
    /// </summary>
    public T State { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public Func<Task> RunStep { get; set; } = new(DoNotRun);

    /// <summary>
    /// Function to be called to check if the step needs to be run by the user
    /// </summary>
    public Func<Task<bool>> ShouldRunStep { get; set; } = new(DoNotRun);

    public static async Task<bool> DoNotRun()
    {
        return await Task.FromResult(false);
    }

    public static async Task<bool> AlwaysRun()
    {
        return await Task.FromResult(true);
    }
}

public class StateMachineStep<T, Q> : StateMachineStep<T>
    where T : struct, IConvertible 
{
    public new Func<Q, Task<bool>> ShouldRunStep { get; set; } = new(DoNotRun);

    public new Func<Q, Task> RunStep { get; set; } = new(DoNotRun);

    public new static Func<Q, Task<bool>> DoNotRun = async (Q _) => await StateMachineStep<T>.DoNotRun();
    public new static Func<Q, Task<bool>> AlwaysRun = async (Q _) => await StateMachineStep<T>.AlwaysRun();
}
