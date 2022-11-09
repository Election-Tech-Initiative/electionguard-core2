using System;
using System.Runtime.InteropServices;

namespace ElectionGuard
{
    /// <summary>
    /// Data entity for the ballot selections in a contest,
    /// for example linking candidates and parties to their vote counts.
    /// See: https://developers.google.com/elections-data/reference/ballot-selection
    ///
    /// Note: The ElectionGuard Data Spec deviates from the NIST model in that
    /// there is no difference for different types of selections.
    ///
    /// The ElectionGuard Data Spec deviates from the NIST model in that
    /// `sequence_order` is a required field since it is used for ordering selections
    /// in a contest to ensure various encryption primitives are deterministic.
    /// For a given election, the sequence of selections displayed to a user may be different
    /// however that information is not captured by default when encrypting a specific ballot.
    /// </summary>
    public class SelectionDescription : DisposableBase
    {
        /// <Summary>
        /// Unique internal identifier that's used by other elements to reference this element.
        /// </Summary>
        public unsafe string ObjectId
        {
            get
            {
                var status = NativeInterface.SelectionDescription.GetObjectId(
                    Handle, out IntPtr value);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"SelectionDescription Error ObjectId: {status}");
                }
                var data = Marshal.PtrToStringAnsi(value);
                NativeInterface.Memory.FreeIntPtr(value);
                return data;
            }
        }

        /// <Summary>
        /// the object id of the candidate
        /// </Summary>
        public unsafe string CandidateId
        {
            get
            {
                var status = NativeInterface.SelectionDescription.GetCandidateId(
                    Handle, out IntPtr value);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"SelectionDescription Error CandidateId: {status}");
                }
                var data = Marshal.PtrToStringAnsi(value);
                NativeInterface.Memory.FreeIntPtr(value);
                return data;
            }
        }

        /// <Summary>
        /// The sequence order defining this selections place in the contest selection collection.
        /// Note: this is specifically for programs to interpret and does not necessarily represent
        /// the order in which selections are presented to a user.
        /// </Summary>
        public unsafe ulong SequenceOrder
        {
            get
            {
                var value = NativeInterface.SelectionDescription.GetSequenceOrder(Handle);
                return value;
            }
        }

        internal unsafe NativeInterface.SelectionDescription.SelectionDescriptionHandle Handle;

        unsafe internal SelectionDescription(
            NativeInterface.SelectionDescription.SelectionDescriptionHandle handle)
        {
            Handle = handle;
        }

        /// <summary>
        /// Create a `SelectionDescription` object
        /// </summary>
        /// <param name="objectId">string identifying the object</param>
        /// <param name="candidateId">string identifying the candidate</param>
        /// <param name="sequenceOrder">the number of the item for sequence order</param>
        public unsafe SelectionDescription(
            string objectId, string candidateId, ulong sequenceOrder)
        {
            var status = NativeInterface.SelectionDescription.New(
                objectId, candidateId, sequenceOrder, out Handle);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"SelectionDescription Error Status: {status}");
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        protected override unsafe void DisposeUnmanaged()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            base.DisposeUnmanaged();

            if (Handle == null || Handle.IsInvalid) return;
            Handle.Dispose();
            Handle = null;
        }

        /// <Summary>
        /// A hash representation of the object
        /// </Summary>
        public unsafe ElementModQ CryptoHash()
        {
            var status = NativeInterface.SelectionDescription.CryptoHash(
                Handle, out NativeInterface.ElementModQ.ElementModQHandle value);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"CryptoHash Error Status: {status}");
            }
            return new ElementModQ(value);
        }
    }
}