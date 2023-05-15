using ElectionGuard.Base;

namespace ElectionGuard.Guardians
{
    /// <summary>
    /// An object representing a guardian that is associated with an election.
    ///
    /// In order for a guardian to be part of an election, 
    /// they must have participated in a key ceremony
    /// and therefore must have a sequence order.
    /// </summary>
    public interface IElectionGuardian : IElectionObject
    {
        string GuardianId { get; }
    }
}
