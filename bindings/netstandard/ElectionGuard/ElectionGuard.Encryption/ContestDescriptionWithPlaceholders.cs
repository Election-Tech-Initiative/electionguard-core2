using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ElectionGuard.Ballot;
using ElectionGuard.Extensions;
using Newtonsoft.Json;

namespace ElectionGuard
{
    /// <summary>
    /// ContestDescriptionWithPlaceholders is a `ContestDescription` with ElectionGuard `placeholder_selections`.
    /// (The ElectionGuard spec requires for n-of-m elections that there be *exactly* n counters that are one
    /// with the rest zero, so if a voter deliberately undervotes, one or more of the placeholder counters will
    /// become one. This allows the `ConstantChaumPedersenProof` to verify correctly for undervoted contests.)
    /// </summary>
    public class ContestDescriptionWithPlaceholders : DisposableBase, IElectionContest
    {
        /// <Summary>
        /// Unique internal identifier that's used by other elements to reference this element.
        /// </Summary>
        public string ObjectId
        {
            get
            {
                var status = NativeInterface.ContestDescriptionWithPlaceholders.GetObjectId(
                    Handle, out var value);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"ContestDescriptionWithPlaceholders Error ObjectId: {status}");
                }
                var data = Marshal.PtrToStringAnsi(value);
                _ = NativeInterface.Memory.FreeIntPtr(value);
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
                var status = NativeInterface.ContestDescriptionWithPlaceholders.GetElectoralDistrictId(
                    Handle, out var value);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"ContestDescriptionWithPlaceholders Error ElectoralDistrictId: {status}");
                }
                var data = Marshal.PtrToStringAnsi(value);
                _ = NativeInterface.Memory.FreeIntPtr(value);
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
                var value = NativeInterface.ContestDescriptionWithPlaceholders.GetSequenceOrder(Handle);
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
                var value = NativeInterface.ContestDescriptionWithPlaceholders.GetVoteVariationType(Handle);
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
                var value = NativeInterface.ContestDescriptionWithPlaceholders.GetNumberElected(Handle);
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
                var value = NativeInterface.ContestDescriptionWithPlaceholders.GetVotesAllowed(Handle);
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
                var status = NativeInterface.ContestDescriptionWithPlaceholders.GetName(
                    Handle, out var value);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"ContestDescriptionWithPlaceholders Error Name: {status}");
                }
                var data = Marshal.PtrToStringAnsi(value);
                _ = NativeInterface.Memory.FreeIntPtr(value);
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
                var status = NativeInterface.ContestDescriptionWithPlaceholders.GetBallotTitle(
                    Handle, out var value);
                return status != Status.ELECTIONGUARD_STATUS_SUCCESS
                    ? throw new ElectionGuardException($"ContestDescriptionWithPlaceholders Error BallotTitle: {status}")
                    : new InternationalizedText(value);
            }
        }

        /// <Summary>
        /// Subtitle of the contest, which must match how it appears on the voters' ballots.
        /// </Summary>
        public InternationalizedText BallotSubTitle
        {
            get
            {
                var status = NativeInterface.ContestDescriptionWithPlaceholders.GetBallotSubTitle(
                    Handle, out var value);
                return status != Status.ELECTIONGUARD_STATUS_SUCCESS
                    ? throw new ElectionGuardException($"ContestDescriptionWithPlaceholders Error BallotSubTitle: {status}")
                    : new InternationalizedText(value);
            }
        }

        /// <Summary>
        /// The size of the selections collection
        /// </Summary>
        public ulong SelectionsSize
        {
            get
            {
                var value = NativeInterface.ContestDescriptionWithPlaceholders.GetSelectionsSize(Handle);
                return value;
            }
        }

        /// <Summary>
        /// The collection of selections for this contest
        /// </Summary>
        public IReadOnlyList<SelectionDescription> Selections =>
            new ElectionGuardEnumerator<SelectionDescription>(
                () => (int)SelectionsSize,
                (index) => GetSelectionAtIndex((ulong)index)
            );

        /// <Summary>
        /// The size of the placeholder collection
        /// </Summary>
        public ulong PlaceholdersSize
        {
            get
            {
                var value = NativeInterface.ContestDescriptionWithPlaceholders.GetPlaceholdersSize(Handle);
                return value;
            }
        }

        public ElementModQ DescriptionHash => CryptoHash();

        /// <Summary>
        /// The collection of placeholders for this contest
        /// </Summary>
        public IReadOnlyList<SelectionDescription> Placeholders =>
            new ElectionGuardEnumerator<SelectionDescription>(
                () => (int)PlaceholdersSize,
                (index) => GetPlaceholderAtIndex((ulong)index)
            );

        internal NativeInterface.ContestDescriptionWithPlaceholders.ContestDescriptionWithPlaceholdersHandle Handle;

        internal ContestDescriptionWithPlaceholders(
            NativeInterface.ContestDescriptionWithPlaceholders.ContestDescriptionWithPlaceholdersHandle handle)
        {
            Handle = handle;
        }

        /// <summary>
        /// Create a `ContestDescriptionWithPlaceholders` object
        /// </summary>
        /// <param name="objectId">string identifying object</param>
        /// <param name="electoralDistrictId">string identifying electoral district</param>
        /// <param name="sequenceOrder">the sequence order to show this in</param>
        /// <param name="voteVariation">vote variation type</param>
        /// <param name="numberElected">the number of elected</param>
        /// <param name="name">string for name of the contest</param>
        /// <param name="selections">array of `SelectionDescription`</param>
        /// <param name="placeholders">array of `SelectionDescription` to use as placeholders</param>
        public ContestDescriptionWithPlaceholders(
            string objectId, string electoralDistrictId, ulong sequenceOrder,
            VoteVariationType voteVariation, ulong numberElected, string name,
            SelectionDescription[] selections, SelectionDescription[] placeholders)
        {
            var selectionPointers = new IntPtr[selections.Length];
            for (var i = 0; i < selections.Length; i++)
            {
                selectionPointers[i] = selections[i].Handle.Ptr;
                selections[i].Dispose();
            }

            var placeholderPointers = new IntPtr[placeholders.Length];
            for (var i = 0; i < placeholders.Length; i++)
            {
                placeholderPointers[i] = placeholders[i].Handle.Ptr;
                placeholders[i].Dispose();
            }

            var status = NativeInterface.ContestDescriptionWithPlaceholders.New(
                objectId, electoralDistrictId, sequenceOrder,
                voteVariation, numberElected, name,
                selectionPointers, (ulong)selectionPointers.LongLength,
                placeholderPointers, (ulong)placeholderPointers.LongLength,
                out Handle);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"ContestDescriptionWithPlaceholders Error Status: {status}");
            }
        }

        /// <summary>
        /// Create a `ContestDescriptionWithPlaceholders` object
        /// </summary>
        /// <param name="objectId">string identifying object</param>
        /// <param name="electoralDistrictId">string identifying electoral district</param>
        /// <param name="sequenceOrder">the sequence order to show this in</param>
        /// <param name="voteVariation">vote variation type</param>
        /// <param name="numberElected">the number of elected</param>
        /// <param name="votesAllowed">number of votes allowed</param>
        /// <param name="name">string for name of the contest</param>
        /// <param name="ballotTitle">international string for the ballot title</param>
        /// <param name="ballotSubtitle">international string for the ballot title</param>
        /// <param name="selections">array of `SelectionDescription`</param>
        /// <param name="placeholders">array of `SelectionDescription` to use as placeholders</param>
        [JsonConstructor]
        public ContestDescriptionWithPlaceholders(
            string objectId, string electoralDistrictId, ulong sequenceOrder,
            VoteVariationType voteVariation, ulong numberElected, ulong votesAllowed,
            string name, InternationalizedText ballotTitle, InternationalizedText ballotSubtitle,
            Dictionary<string, SelectionDescription> selections, Dictionary<string, SelectionDescription> placeholders)
        {
            var selectionPointers = new IntPtr[selections.Values.Count];
            foreach (var (selection, index) in selections.Values.WithIndex())
            {
                selectionPointers[index] = selection.Handle.Ptr;
                selection.Dispose();
            }

            var placeholderPointers = new IntPtr[placeholders.Values.Count];
            foreach (var (placeholder, index) in placeholders.Values.WithIndex())
            {
                placeholderPointers[index] = placeholder.Handle.Ptr;
                placeholder.Dispose();
            }

            var status = NativeInterface.ContestDescriptionWithPlaceholders.New(
                objectId, electoralDistrictId, sequenceOrder,
                voteVariation, numberElected, votesAllowed,
                name, ballotTitle.Handle, ballotSubtitle.Handle,
                selectionPointers, (ulong)selectionPointers.LongLength,
                placeholderPointers, (ulong)placeholderPointers.LongLength,
                out Handle);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"ContestDescriptionWithPlaceholders Error Status: {status}");
            }
        }


        /// <summary>
        /// Create a `ContestDescriptionWithPlaceholders` object
        /// </summary>
        /// <param name="objectId">string identifying object</param>
        /// <param name="electoralDistrictId">string identifying electoral district</param>
        /// <param name="sequenceOrder">the sequence order to show this in</param>
        /// <param name="voteVariation">vote variation type</param>
        /// <param name="numberElected">the number of elected</param>
        /// <param name="votesAllowed">number of votes allowed</param>
        /// <param name="name">string for name of the contest</param>
        /// <param name="ballotTitle">international string for the ballot title</param>
        /// <param name="ballotSubtitle">international string for the ballot title</param>
        /// <param name="selections">array of `SelectionDescription`</param>
        /// <param name="placeholders">array of `SelectionDescription` to use as placeholders</param>
        public ContestDescriptionWithPlaceholders(
            string objectId, string electoralDistrictId, ulong sequenceOrder,
            VoteVariationType voteVariation, ulong numberElected, ulong votesAllowed,
            string name, InternationalizedText ballotTitle, InternationalizedText ballotSubtitle,
            SelectionDescription[] selections, SelectionDescription[] placeholders)
        {
            var selectionPointers = new IntPtr[selections.Length];
            for (var i = 0; i < selections.Length; i++)
            {
                selectionPointers[i] = selections[i].Handle.Ptr;
                selections[i].Dispose();
            }

            var placeholderPointers = new IntPtr[placeholders.Length];
            for (var i = 0; i < placeholders.Length; i++)
            {
                placeholderPointers[i] = placeholders[i].Handle.Ptr;
                placeholders[i].Dispose();
            }

            var status = NativeInterface.ContestDescriptionWithPlaceholders.New(
                objectId, electoralDistrictId, sequenceOrder,
                voteVariation, numberElected, votesAllowed,
                name, ballotTitle.Handle, ballotSubtitle.Handle,
                selectionPointers, (ulong)selectionPointers.LongLength,
                placeholderPointers, (ulong)placeholderPointers.LongLength,
                out Handle);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"ContestDescriptionWithPlaceholders Error Status: {status}");
            }
        }


        /// <summary>
        /// Create a `ContestDescriptionWithPlaceholders` object
        /// </summary>
        /// <param name="objectId">string identifying object</param>
        /// <param name="electoralDistrictId">string identifying electoral district</param>
        /// <param name="sequenceOrder">the sequence order to show this in</param>
        /// <param name="voteVariation">vote variation type</param>
        /// <param name="numberElected">the number of elected</param>
        /// <param name="name">string for name of the contest</param>
        /// <param name="selections">array of `SelectionDescription`</param>
        /// <param name="primaryPartyIds">array of strings for `PartyIds`</param>
        /// <param name="placeholders">array of `SelectionDescription` to use as placeholders</param>
        public ContestDescriptionWithPlaceholders(
            string objectId, string electoralDistrictId, ulong sequenceOrder,
            VoteVariationType voteVariation, ulong numberElected, string name,
            SelectionDescription[] selections, string[] primaryPartyIds,
            SelectionDescription[] placeholders)
        {
            var selectionPointers = new IntPtr[selections.Length];
            for (var i = 0; i < selections.Length; i++)
            {
                selectionPointers[i] = selections[i].Handle.Ptr;
                selections[i].Dispose();
            }

            var placeholderPointers = new IntPtr[placeholders.Length];
            for (var i = 0; i < placeholders.Length; i++)
            {
                placeholderPointers[i] = placeholders[i].Handle.Ptr;
                placeholders[i].Dispose();
            }

            var status = NativeInterface.ContestDescriptionWithPlaceholders.New(
                objectId, electoralDistrictId, sequenceOrder,
                voteVariation, numberElected, name,
                selectionPointers, (ulong)selectionPointers.LongLength,
                primaryPartyIds, (ulong)primaryPartyIds.LongLength,
                placeholderPointers, (ulong)placeholderPointers.LongLength,
                out Handle);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"ContestDescriptionWithPlaceholders Error Status: {status}");
            }
        }

        /// <summary>
        /// Create a `ContestDescriptionWithPlaceholders` object
        /// </summary>
        /// <param name="objectId">string identifying object</param>
        /// <param name="electoralDistrictId">string identifying electoral district</param>
        /// <param name="sequenceOrder">the sequence order to show this in</param>
        /// <param name="voteVariation">vote variation type</param>
        /// <param name="numberElected">the number of elected</param>
        /// <param name="votesAllowed">number of votes allowed</param>
        /// <param name="name">string for name of the contest</param>
        /// <param name="ballotTitle">international string for the ballot title</param>
        /// <param name="ballotSubtitle">international string for the ballot title</param>
        /// <param name="selections">array of `SelectionDescription`</param>
        /// <param name="primaryPartyIds">array of strings for `PartyIds`</param>
        /// <param name="placeholders">array of `SelectionDescription` to use as placeholders</param>
        public ContestDescriptionWithPlaceholders(
            string objectId, string electoralDistrictId, ulong sequenceOrder,
            VoteVariationType voteVariation, ulong numberElected, ulong votesAllowed,
            string name, InternationalizedText ballotTitle, InternationalizedText ballotSubtitle,
            SelectionDescription[] selections, string[] primaryPartyIds,
            SelectionDescription[] placeholders)
        {
            var selectionPointers = new IntPtr[selections.Length];
            for (var i = 0; i < selections.Length; i++)
            {
                selectionPointers[i] = selections[i].Handle.Ptr;
                selections[i].Dispose();
            }

            var placeholderPointers = new IntPtr[placeholders.Length];
            for (var i = 0; i < placeholders.Length; i++)
            {
                placeholderPointers[i] = placeholders[i].Handle.Ptr;
                placeholders[i].Dispose();
            }

            var status = NativeInterface.ContestDescriptionWithPlaceholders.New(
                objectId, electoralDistrictId, sequenceOrder,
                voteVariation, numberElected, votesAllowed,
                name, ballotTitle.Handle, ballotSubtitle.Handle,
                selectionPointers, (ulong)selectionPointers.LongLength,
                primaryPartyIds, (ulong)primaryPartyIds.LongLength,
                placeholderPointers, (ulong)placeholderPointers.LongLength,
                out Handle);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"ContestDescriptionWithPlaceholders Error Status: {status}");
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        protected override void DisposeUnmanaged()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            base.DisposeUnmanaged();

            if (Handle == null || Handle.IsInvalid)
            {
                return;
            }

            Handle.Dispose();
            Handle = null;
        }

        /// <Summary>
        /// The collection of selections in this contest.
        /// </Summary>
        public SelectionDescription GetSelectionAtIndex(ulong index)
        {
            var status = NativeInterface.ContestDescriptionWithPlaceholders.GetSelectionAtIndex(
                Handle, index, out var value);
            return status != Status.ELECTIONGUARD_STATUS_SUCCESS
                ? throw new ElectionGuardException($"ContestDescriptionWithPlaceholders ContestDescription GetSelectionAtIndex: {status}")
                : new SelectionDescription(value);
        }

        /// <Summary>
        /// The collection of placeholders in this contest.
        /// </Summary>
        public SelectionDescription GetPlaceholderAtIndex(ulong index)
        {
            var status = NativeInterface.ContestDescriptionWithPlaceholders.GetPlaceholderAtIndex(
                Handle, index, out var value);
            return status != Status.ELECTIONGUARD_STATUS_SUCCESS
                ? throw new ElectionGuardException($"ContestDescriptionWithPlaceholders ContestDescription GetSelectionAtIndex: {status}")
                : new SelectionDescription(value);
        }

        /// <Summary>
        /// A hash representation of the object
        /// </Summary>
        public ElementModQ CryptoHash()
        {
            var status = NativeInterface.ContestDescriptionWithPlaceholders.CryptoHash(
                Handle, out var value);
            return status != Status.ELECTIONGUARD_STATUS_SUCCESS
                ? throw new ElectionGuardException($"ContestDescriptionWithPlaceholders Error CryptoHash: {status}")
                : new ElementModQ(value);
        }
    }
}
