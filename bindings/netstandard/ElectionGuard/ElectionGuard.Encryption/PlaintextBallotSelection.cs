namespace ElectionGuard
{
    /// <summary>
    /// A BallotSelection represents an individual selection on a ballot.
    ///
    /// This class accepts a `vote` integer field which has no constraints
    /// in the ElectionGuard Data Specification, but is constrained logically
    /// in the application to resolve to `True` or `False`aka only 0 and 1 is
    /// supported for now.
    ///
    /// This class can also be designated as `isPlaceholderSelection` which has no
    /// context to the data specification but is useful for running validity checks internally
    ///
    /// an `extendedData` field exists to support any arbitrary data to be associated
    /// with the selection.  In practice, this field is the cleartext representation
    /// of a write-in candidate value.  In the current implementation these values are
    /// discarded when encrypting.
    /// </summary>
    public partial class PlaintextBallotSelection : DisposableBase
    {
        internal PlaintextBallotSelection(External.PlaintextBallotSelectionHandle handle)
        {
            Handle = handle;
        }

        /// <summary>
        /// Create a PlaintextBallotSelection
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="vote"></param>
        /// <param name="isPlaceholder"></param>
        public PlaintextBallotSelection(
            string objectId, ulong vote, bool isPlaceholder = false)
        {
            var status = NativeInterface.PlaintextBallotSelection.New(
                objectId, vote, isPlaceholder, out Handle);
            status.ThrowIfError();
        }

        /// <summary>
        /// Create a PlaintextBallotSelection
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="vote"></param>
        /// <param name="isPlaceholder"></param>
        /// <param name="extendedData"></param>
        public PlaintextBallotSelection(
            string objectId, ulong vote, bool isPlaceholder, string extendedData)
        {
            var status = NativeInterface.PlaintextBallotSelection.New(
                objectId, vote, isPlaceholder,
                extendedData, (ulong)extendedData.Length, out Handle);
            status.ThrowIfError();
        }
    }
}