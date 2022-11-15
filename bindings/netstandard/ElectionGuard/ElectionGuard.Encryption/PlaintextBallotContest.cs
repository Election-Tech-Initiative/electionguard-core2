using System;

namespace ElectionGuard
{
    /// <summary>
    /// A PlaintextBallotContest represents the selections made by a voter for a specific ContestDescription
    ///
    /// This class can be either a partial or a complete representation of a contest dataset.  Specifically,
    /// a partial representation must include at a minimum the "affirmative" selections of a contest.
    /// A complete representation of a ballot must include both affirmative and negative selections of
    /// the contest, AND the placeholder selections necessary to satisfy the ConstantChaumPedersen proof
    /// in the CiphertextBallotContest.
    ///
    /// Typically partial contests may be passed into ElectionGuard for memory constrained systems,
    /// while complete contests are passed into ElectionGuard when running encryption on an existing dataset.
    /// </summary>
    public partial class PlaintextBallotContest : DisposableBase
    {
        internal PlaintextBallotContest(
            External.PlaintextBallotContestHandle handle)
        {
            Handle = handle;
        }

        /// <summary>
        /// Create a PlaintextBallotContest
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="selections"></param>
        public PlaintextBallotContest(string objectId, PlaintextBallotSelection[] selections)
        {
            IntPtr[] selectionPointers = new IntPtr[selections.Length];
            for (var i = 0; i < selections.Length; i++)
            {
                selectionPointers[i] = selections[i].Handle.Ptr;
                selections[i].Dispose();
            }

            var status = NativeInterface.PlaintextBallotContest.New(
                objectId, selectionPointers, (ulong)selectionPointers.LongLength, out Handle);
            status.ThrowIfError();
        }
    }
}