using System;
using System.Runtime.InteropServices;

namespace ElectionGuard
{
    /// <Summary>
    /// Entity describing information about a candidate in a contest.
    /// See: https://developers.google.com/elections-data/reference/candidate
    ///
    /// Note: The ElectionGuard Data Spec deviates from the NIST model in that
    /// selections for any contest type are considered a "candidate".
    /// for instance, on a yes-no referendum contest, two `candidate` objects
    /// would be included in the model to represent the `affirmative` and `negative`
    /// selections for the contest.  See the wiki, readme, and tests in this repo for more info.
    /// </Summary>
    public class Candidate : DisposableBase
    {
        /// <Summary>
        /// Unique internal identifier that's used by other elements to reference this element.
        /// </Summary>
        public string ObjectId
        {
            get
            {
                var status = NativeInterface.Candidate.GetObjectId(
                    Handle, out IntPtr value);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"Candidate Error ObjectId: {status}");
                }
                var data = Marshal.PtrToStringAnsi(value);
                NativeInterface.Memory.FreeIntPtr(value);
                return data;
            }
        }

        /// <Summary>
        /// A convenience accessor for getObjectId
        /// </Summary>
        public string CandidateId
        {
            get
            {
                var status = NativeInterface.Candidate.GetCandidateId(
                    Handle, out IntPtr value);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"Candidate Error CandidateId: {status}");
                }
                var data = Marshal.PtrToStringAnsi(value);
                NativeInterface.Memory.FreeIntPtr(value);
                return data;
            }
        }

        /// <Summary>
        /// Name of the candidate
        /// </Summary>
        public InternationalizedText Name
        {
            get
            {
                var status = NativeInterface.Candidate.GetName(
                    Handle, out NativeInterface.InternationalizedText.InternationalizedTextHandle value);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"Candidate Error Name: {status}");
                }
                return new InternationalizedText(value);
            }
        }

        /// <Summary>
        /// Optional party id of the candidate
        /// </Summary>
        public string PartyId
        {
            get
            {
                var status = NativeInterface.Candidate.GetPartyId(
                    Handle, out IntPtr value);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"Candidate Error PartyId: {status}");
                }
                var data = Marshal.PtrToStringAnsi(value);
                NativeInterface.Memory.FreeIntPtr(value);
                return data;
            }
        }

        /// <Summary>
        /// Optional image uri for the candidate
        /// </Summary>
        public string ImageUri
        {
            get
            {
                var status = NativeInterface.Candidate.GetImageUri(
                    Handle, out IntPtr value);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"Candidate Error ImageUri: {status}");
                }
                var data = Marshal.PtrToStringAnsi(value);
                NativeInterface.Memory.FreeIntPtr(value);
                return data;
            }
        }

        public bool IsWriteIn => NativeInterface.Candidate.GetIsWriteIn(Handle);

        internal NativeInterface.Candidate.CandidateHandle Handle;

        internal Candidate(
            NativeInterface.Candidate.CandidateHandle handle)
        {
            Handle = handle;
        }

        /// <summary>
        /// Create a `Candidate` object
        /// </summary>
        /// <param name="objectId">string for the identity of the object</param>
        /// <param name="isWriteIn">is the candidate a write in</param>
        public Candidate(string objectId, bool isWriteIn)
        {
            var status = NativeInterface.Candidate.New(objectId, isWriteIn, out Handle);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"Candidate Error Status: {status}");
            }
        }

        /// <summary>
        /// Create a `Candidate` object
        /// </summary>
        /// <param name="objectId">string for the identity of the object</param>
        /// <param name="partyId">string identifying the party for the candidate</param>
        /// <param name="isWriteIn">is the candidate a write in</param>
        public Candidate(string objectId, string partyId, bool isWriteIn)
        {
            var status = NativeInterface.Candidate.New(
                objectId, partyId, isWriteIn, out Handle);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"Candidate Error Status: {status}");
            }
        }

        /// <summary>
        /// Create a `Candidate` object
        /// </summary>
        /// <param name="objectId">string for the identity of the object</param>
        /// <param name="name">name of the candidate</param>
        /// <param name="partyId">string identifying the party for the candidate</param>
        /// <param name="imageUri">string for uir for image of candidate</param>
        /// <param name="isWriteIn">is the candidate a write in</param>
        public Candidate(
            string objectId, InternationalizedText name,
            string partyId, string imageUri, bool isWriteIn)
        {
            var status = NativeInterface.Candidate.New(
                objectId, name.Handle, partyId, imageUri ?? string.Empty, isWriteIn, out Handle);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"Candidate Error Status: {status}");
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        protected override void DisposeUnmanaged()
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
        public ElementModQ CryptoHash()
        {
            var status = NativeInterface.Candidate.CryptoHash(
                Handle, out NativeInterface.ElementModQ.ElementModQHandle value);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"CryptoHash Error Status: {status}");
            }
            return new ElementModQ(value);
        }
    }
}
