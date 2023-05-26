using ElectionGuard.UI.Lib.Models;

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

    private static async Task<bool> DoNotRun()
    {
        return await Task.FromResult(false);
    }
}

