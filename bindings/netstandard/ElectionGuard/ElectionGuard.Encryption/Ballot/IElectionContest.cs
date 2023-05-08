using ElectionGuard.Base;

namespace ElectionGuard.Ballot
{
    /// <summary>
    /// A contest on a ballot or in a tally
    /// </summary>
    public interface IElectionContest : IElectionObject
    {
        /// <summary>
        /// The description hash
        /// </summary>
        ElementModQ DescriptionHash { get; }
    }
}
