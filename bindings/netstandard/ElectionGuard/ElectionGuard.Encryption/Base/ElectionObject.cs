namespace ElectionGuard.Base
{
    /// <summary>
    /// A common interface for election objects
    /// </summary>
    public interface IElectionObject
    {
        /// <summary>
        /// The object id
        /// </summary>
        string ObjectId { get; }

        /// <summary>
        /// The sequence order
        /// </summary>
        ulong SequenceOrder { get; }
    }
}
