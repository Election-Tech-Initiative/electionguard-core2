using ElectionGuard.Base;

namespace ElectionGuard.Ballot
{
    /// <summary>
    /// A selection on a ballot or in a tally
    /// </summary>
    public interface IElectionSelection : IElectionObject
    {
        /// <summary>
        /// The description hash
        /// </summary>
        ElementModQ DescriptionHash { get; }
    }
}
