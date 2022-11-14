using System;
using System.Runtime.InteropServices;

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
        /// Create a Plaintext Ballot
        /// </summary>
        /// <param name="json">json representation</param>
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
            IntPtr[] contestPointers = new IntPtr[contests.Length];
            for (var i = 0; i < contests.Length; i++)
            {
                contestPointers[i] = contests[i].Handle.Ptr;
                contests[i].Dispose();
            }

            var status = NativeInterface.PlaintextBallot.New(
                objectId, styleId, contestPointers, (ulong)contestPointers.LongLength, out Handle);
            status.ThrowIfError();
        }

        /// <summary>
        /// Get the contest at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public PlaintextBallotContest GetContestAt(ulong index)
        {
            var status = NativeInterface.PlaintextBallot.GetContestAtIndex(
                Handle, index, out PlaintextBallotContest.External.PlaintextBallotContestHandle value);
            status.ThrowIfError();
            return new PlaintextBallotContest(value);
        }

        /// <Summary>
        /// Export the ballot representation as JSON
        /// </Summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
        public string ToJson()
        {
            var status = NativeInterface.PlaintextBallot.ToJson(
                Handle, out IntPtr pointer, out _);
            status.ThrowIfError();
            var json = Marshal.PtrToStringAnsi(pointer);
            NativeInterface.Memory.FreeIntPtr(pointer);
            return json;
        }

        /// <Summary>
        /// Export the ballot representation as BSON
        /// </Summary>
        public byte[] ToBson()
        {

            var status = NativeInterface.PlaintextBallot.ToBson(
                Handle, out IntPtr data, out ulong size);
            status.ThrowIfError();

            if (size > int.MaxValue)
            {
                throw new ElectionGuardException("PlaintextBallot Error ToBson: size is too big");
            }

            var byteArray = new byte[(int)size];
            Marshal.Copy(data, byteArray, 0, (int)size);
            NativeInterface.Memory.DeleteIntPtr(data);
            return byteArray;
        }

        /// <Summary>
        /// Export the ballot representation as MsgPack
        /// </Summary>
        public byte[] ToMsgPack()
        {

            var status = NativeInterface.PlaintextBallot.ToMsgPack(
                Handle, out IntPtr data, out ulong size);
            status.ThrowIfError();

            if (size > int.MaxValue)
            {
                throw new ElectionGuardException("PlaintextBallot Error ToMsgPack: size is too big");
            }

            var byteArray = new byte[(int)size];
            Marshal.Copy(data, byteArray, 0, (int)size);
            NativeInterface.Memory.DeleteIntPtr(data);
            return byteArray;
        }

    }
}