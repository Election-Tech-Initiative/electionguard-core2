using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ElectionGuard
{
    /// <summary>
    /// Use this data entity for describing a contest and linking the contest
    /// to the associated candidates and parties.
    /// See: https://developers.google.com/elections-data/reference/contest
    /// Note: The ElectionGuard Data Spec deviates from the NIST model in that
    /// `sequence_order` is a required field since it is used for ordering selections
    /// in a contest to ensure various encryption primitives are deterministic.
    /// For a given election, the sequence of contests displayed to a user may be different
    /// however that information is not captured by default when encrypting a specific ballot.
    /// </summary>
    public class ContestDescription : DisposableBase, IReadOnlyList<SelectionDescription>
    {
        /// <Summary>
        /// Unique internal identifier that's used by other elements to reference this element.
        /// </Summary>
        public string ObjectId
        {
            get
            {
                var status = NativeInterface.ContestDescription.GetObjectId(
                    Handle, out IntPtr value);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"ContestDescription Error ObjectId: {status}");
                }
                var data = Marshal.PtrToStringAnsi(value);
                NativeInterface.Memory.FreeIntPtr(value);
                return data;
            }
        }

        /// <Summary>
        /// The object id of the geopolitical unit associated with this contest.
        /// Note: in concordance with the NIST standard, the name `ElectoralDistrictId` is kept
        /// </Summary>
        public string ElectoralDistrictId
        {
            get
            {
                var status = NativeInterface.ContestDescription.GetElectoralDistrictId(
                    Handle, out IntPtr value);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"ContestDescription Error ElectoralDistrictId: {status}");
                }
                var data = Marshal.PtrToStringAnsi(value);
                NativeInterface.Memory.FreeIntPtr(value);
                return data;
            }
        }

        /// <Summary>
        /// The sequence order defining this contest's place in the contest collection of the ballot style.
        /// Note: this is specifically for programs to interpret and does not necessarily represent
        /// the order in which contests are presented to a user.
        /// </Summary>
        public ulong SequenceOrder
        {
            get
            {
                var value = NativeInterface.ContestDescription.GetSequenceOrder(Handle);
                return value;
            }
        }

        /// <Summary>
        /// The vote variation type.  Currently ElectionGuard supports one_of_m and n_of_m
        /// </Summary>
        public VoteVariationType VoteVariationType
        {
            get
            {
                var value = NativeInterface.ContestDescription.GetVoteVariationType(Handle);
                return value;
            }
        }

        /// <Summary>
        /// The number of candidates that are elected in the contest, which is the n of an n-of-m contest
        /// </Summary>
        public ulong NumberElected
        {
            get
            {
                var value = NativeInterface.ContestDescription.GetNumberElected(Handle);
                return value;
            }
        }

        /// <Summary>
        /// The maximum number of votes or write-ins allowed per voter in this contest.
        /// </Summary>
        public ulong VotesAllowed
        {
            get
            {
                var value = NativeInterface.ContestDescription.GetVotesAllowed(Handle);
                return value;
            }
        }

        /// <Summary>
        /// Name of the contest as it's listed on the results report,
        /// not necessarily as it appears on the ballot.
        /// </Summary>
        public string Name
        {
            get
            {
                var status = NativeInterface.ContestDescription.GetName(
                    Handle, out IntPtr value);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"ContestDescription Error Name: {status}");
                }
                var data = Marshal.PtrToStringAnsi(value);
                NativeInterface.Memory.FreeIntPtr(value);
                return data;
            }
        }

        /// <Summary>
        /// Title of the contest, which must match how it appears on the voters' ballots.
        /// </Summary>
        public InternationalizedText BallotTitle
        {
            get
            {
                var status = NativeInterface.ContestDescription.GetBallotTitle(
                    Handle, out NativeInterface.InternationalizedText.InternationalizedTextHandle value);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"ContestDescription Error BallotTitle: {status}");
                }
                return new InternationalizedText(value);
            }
        }

        /// <Summary>
        /// Subtitle of the contest, which must match how it appears on the voters' ballots.
        /// </Summary>
        public InternationalizedText BallotSubTitle
        {
            get
            {
                var status = NativeInterface.ContestDescription.GetBallotSubTitle(
                    Handle, out NativeInterface.InternationalizedText.InternationalizedTextHandle value);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"ContestDescription Error BallotSubTitle: {status}");
                }
                return new InternationalizedText(value);
            }
        }

        /// <Summary>
        /// The size of the selections collection
        /// </Summary>
        public ulong SelectionsSize
        {
            get
            {
                var value = NativeInterface.ContestDescription.GetSelectionsSize(Handle);
                return value;
            }
        }

        internal NativeInterface.ContestDescription.ContestDescriptionHandle Handle;

        internal ContestDescription(
            NativeInterface.ContestDescription.ContestDescriptionHandle handle)
        {
            Handle = handle;
        }

        /// <summary>
        /// Create a `ContestDescription` object
        /// </summary>
        /// <param name="objectId">string identifying object</param>
        /// <param name="electoralDistrictId">string identifying electoral district</param>
        /// <param name="sequenceOrder">the sequence order to show this in</param>
        /// <param name="voteVariation">vote variation type</param>
        /// <param name="numberElected">the number of elected</param>
        /// <param name="name">string for name of the contest</param>
        /// <param name="selections">array of `SelectionDescription`</param>
        public ContestDescription(
            string objectId, string electoralDistrictId, ulong sequenceOrder,
            VoteVariationType voteVariation, ulong numberElected, string name,
            SelectionDescription[] selections)
        {
            IntPtr[] selectionPointers = new IntPtr[selections.Length];
            for (var i = 0; i < selections.Length; i++)
            {
                selectionPointers[i] = selections[i].Handle.Ptr;
                selections[i].Dispose();
            }

            var status = NativeInterface.ContestDescription.New(
                objectId, electoralDistrictId, sequenceOrder,
                voteVariation, numberElected, name,
                selectionPointers, (ulong)selectionPointers.LongLength, out Handle);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"ContestDescription Error Status: {status}");
            }
        }

        /// <summary>
        /// Create a `ContestDescription` object
        /// </summary>
        /// <param name="objectId">string identifying object</param>
        /// <param name="electoralDistrictId">string identifying electoral district</param>
        /// <param name="sequenceOrder">the sequence order to show this in</param>
        /// <param name="voteVariation">vote variation type</param>
        /// <param name="numberElected">number of elected</param>
        /// <param name="votesAllowed">number of votes allowed</param>
        /// <param name="name">string for name of the contest</param>
        /// <param name="ballotTitle">international string for the ballot title</param>
        /// <param name="ballotSubtitle">international string for the ballot title</param>
        /// <param name="selections">array of `SelectionDescription`</param>
        public ContestDescription(
            string objectId, string electoralDistrictId, ulong sequenceOrder,
            VoteVariationType voteVariation, ulong numberElected, ulong votesAllowed,
            string name, InternationalizedText ballotTitle, InternationalizedText ballotSubtitle,
            SelectionDescription[] selections)
        {
            IntPtr[] selectionPointers = new IntPtr[selections.Length];
            for (var i = 0; i < selections.Length; i++)
            {
                selectionPointers[i] = selections[i].Handle.Ptr;
                selections[i].Dispose();
            }

            var status = NativeInterface.ContestDescription.New(
                objectId, electoralDistrictId, sequenceOrder,
                voteVariation, numberElected, votesAllowed,
                name, ballotTitle.Handle, ballotSubtitle.Handle,
                selectionPointers, (ulong)selectionPointers.LongLength, out Handle);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"ContestDescription Error Status: {status}");
            }
        }

        /// <summary>
        /// Create a `ContestDescription` object
        /// </summary>
        /// <param name="objectId">string identifying object</param>
        /// <param name="electoralDistrictId">string identifying electoral district</param>
        /// <param name="sequenceOrder">the sequence order to show this in</param>
        /// <param name="voteVariation">vote variation type</param>
        /// <param name="numberElected">number of elected</param>
        /// <param name="name">string for name of the contest</param>
        /// <param name="selections">array of `SelectionDescription`</param>
        /// <param name="primaryPartyIds">array of strings for `PartyIds`</param>
        public ContestDescription(
            string objectId, string electoralDistrictId, ulong sequenceOrder,
            VoteVariationType voteVariation, ulong numberElected, string name,
            SelectionDescription[] selections, string[] primaryPartyIds)
        {
            IntPtr[] selectionPointers = new IntPtr[selections.Length];
            for (var i = 0; i < selections.Length; i++)
            {
                selectionPointers[i] = selections[i].Handle.Ptr;
                selections[i].Dispose();
            }

            var status = NativeInterface.ContestDescription.New(
                objectId, electoralDistrictId, sequenceOrder,
                voteVariation, numberElected, name,
                selectionPointers, (ulong)selectionPointers.LongLength,
                primaryPartyIds, (ulong)primaryPartyIds.LongLength, out Handle);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"ContestDescription Error Status: {status}");
            }
        }

        /// <summary>
        /// Create a `ContestDescription` object
        /// </summary>
        /// <param name="objectId">string identifying object</param>
        /// <param name="electoralDistrictId">string identifying electoral district</param>
        /// <param name="sequenceOrder">the sequence order to show this in</param>
        /// <param name="voteVariation">vote variation type</param>
        /// <param name="numberElected">number of elected</param>
        /// <param name="votesAllowed">number of votes allowed</param>
        /// <param name="name">string for name of the contest</param>
        /// <param name="ballotTitle">international string for the ballot title</param>
        /// <param name="ballotSubtitle">international string for the ballot title</param>
        /// <param name="selections">array of `SelectionDescription`</param>
        /// <param name="primaryPartyIds">array of strings for `PartyIds`</param>
        public ContestDescription(
            string objectId, string electoralDistrictId, ulong sequenceOrder,
            VoteVariationType voteVariation, ulong numberElected, ulong votesAllowed,
            string name, InternationalizedText ballotTitle, InternationalizedText ballotSubtitle,
            SelectionDescription[] selections, string[] primaryPartyIds)
        {
            IntPtr[] selectionPointers = new IntPtr[selections.Length];
            for (var i = 0; i < selections.Length; i++)
            {
                selectionPointers[i] = selections[i].Handle.Ptr;
                selections[i].Dispose();
            }

            var status = NativeInterface.ContestDescription.New(
                objectId, electoralDistrictId, sequenceOrder,
                voteVariation, numberElected, votesAllowed,
                name, ballotTitle.Handle, ballotSubtitle.Handle,
                selectionPointers, (ulong)selectionPointers.LongLength,
                primaryPartyIds, (ulong)primaryPartyIds.LongLength, out Handle);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"ContestDescription Error Status: {status}");
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
        /// The collection of selections in this contest.
        /// </Summary>
        public SelectionDescription GetSelectionAtIndex(ulong index)
        {
            var status = NativeInterface.ContestDescription.GetSelectionAtIndex(
                Handle, index, out NativeInterface.SelectionDescription.SelectionDescriptionHandle value);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"ContestDescription Error GetSelectionAtIndex: {status}");
            }
            return new SelectionDescription(value);
        }

        /// <Summary>
        /// A hash representation of the object
        /// </Summary>
        public ElementModQ CryptoHash()
        {
            var status = NativeInterface.ContestDescription.CryptoHash(
                Handle, out NativeInterface.ElementModQ.ElementModQHandle value);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"CryptoHash Error Status: {status}");
            }
            return new ElementModQ(value);
        }

        #region IReadOnlyList implementation

        public int Count => (int)SelectionsSize;

        public SelectionDescription this[int index] => GetSelectionAtIndex((ulong)index);


        public IEnumerator<SelectionDescription> GetEnumerator()
        {
            var count = (int)SelectionsSize;
            for (var i = 0; i < count; i++)
            {
                yield return GetSelectionAtIndex((ulong)i);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

    }
}
