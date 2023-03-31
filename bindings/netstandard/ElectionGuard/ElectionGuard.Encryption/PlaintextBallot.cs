using System;
using System.Collections.Generic;

namespace ElectionGuard
{
    /// <summary>
    /// A PlaintextBallot represents a voters selections for a given ballot and ballot style.
    ///
    /// This class can be either a partial or a complete representation of the expected values of a ballot dataset.
    /// Specifically, a partial representation must include at a minimum the "affirmative" selections
    /// of every contest where a selection is made.  A partial representation may exclude contests for which
    /// no selection is made.
    ///
    /// A complete representation of a ballot must include both affirmative and negative selections of
    /// every contest, AND the placeholder selections necessary to satisfy the NIZKPs for each contest and selection.
    /// </summary>
    public partial class PlaintextBallot : DisposableBase
    {
        /// <summary>
        /// the collection of Contests for the ballot
        /// </summary>
        public IReadOnlyList<PlaintextBallotContest> Contests =>
            new ElectionGuardEnumerator<PlaintextBallotContest>(
                () => (int)ContestsSize,
                (index) => GetContestAtIndex((ulong)index)
            );

        /// <summary>
        /// Creates a <see cref="PlaintextBallot">PlaintextBallot</see> object from a <see href="https://www.rfc-editor.org/rfc/rfc8259.html#section-8.1">[RFC-8259]</see> UTF-8 encoded JSON string
        /// </summary>
        /// <param name="json">A UTF-8 Encoded JSON data string</param>
        public PlaintextBallot(string json)
        {
            var status = NativeInterface.PlaintextBallot.FromJson(json, out Handle);
            status.ThrowIfError();
        }

        /// <summary>
        /// Create a Plaintext Ballot
        /// </summary>
        /// <param name="data">binary representation</param>
        /// <param name="encoding">the encoding type</param>
        public unsafe PlaintextBallot(byte[] data, BinarySerializationEncoding encoding)
        {
            fixed (byte* pointer = data)
            {
                var status = encoding == BinarySerializationEncoding.BSON
                    ? NativeInterface.PlaintextBallot.FromBson(pointer, (ulong)data.Length, out Handle)
                    : NativeInterface.PlaintextBallot.FromMsgPack(pointer, (ulong)data.Length, out Handle);
                status.ThrowIfError();
            }
        }

        /// <summary>
        /// Create a PlaintextBallot
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="styleId"></param>
        /// <param name="contests"></param>
        public PlaintextBallot(
            string objectId, string styleId, PlaintextBallotContest[] contests)
        {
            var contestPointers = new IntPtr[contests.Length];
            for (var i = 0; i < contests.Length; i++)
            {
                contestPointers[i] = contests[i].Handle.Ptr;
                contests[i].Dispose();
            }

            var status = NativeInterface.PlaintextBallot.New(
                objectId, styleId, contestPointers, (ulong)contestPointers.LongLength, out Handle);
            status.ThrowIfError();
        }

        public void ForEachContestSelection(
            Action<PlaintextBallotContest, PlaintextBallotSelection> action)
        {
            foreach (var contest in Contests)
            {
                foreach (var selection in contest.Selections)
                {
                    action(contest, selection);
                }
            }
        }

        public PlaintextBallot Copy()
        {
            var json = ToJson();
            return new PlaintextBallot(json);
        }

        public static PlaintextBallot Copy(PlaintextBallot ballot)
        {
            var json = ballot.ToJson();
            return new PlaintextBallot(json);
        }
    }
}
