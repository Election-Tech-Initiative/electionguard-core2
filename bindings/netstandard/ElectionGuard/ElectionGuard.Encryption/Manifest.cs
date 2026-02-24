using System;
using System.Runtime.InteropServices;

namespace ElectionGuard
{
    using NativeGeopoliticalUnit = NativeInterface.GeopoliticalUnit.GeopoliticalUnitHandle;
    using NativeBallotStyle = NativeInterface.BallotStyle.BallotStyleHandle;
    using NativeParty = NativeInterface.Party.PartyHandle;
    using NativeCandidate = NativeInterface.Candidate.CandidateHandle;
    using NativeContestDescription = NativeInterface.ContestDescription.ContestDescriptionHandle;
    using NativeElementModQ = NativeInterface.ElementModQ.ElementModQHandle;

    #region AnnotatedString

    #endregion

    #region Language

    #endregion

    #region InternationalizedText

    #endregion

    #region ContactInformation

    #endregion

    #region GeopoliticalUnit

    #endregion

    #region BallotStyle

    #endregion

    #region Party

    #endregion

    #region Candidate

    #endregion

    #region SelectionDescription

    #endregion

    #region ContestDescription

    #endregion

    #region ContestDescriptionWithPlaceholders

    #endregion

    /// <summary>
    /// Use this entity for defining the structure of the election and associated
    /// information such as candidates, contests, and vote counts.  This class is
    /// based on the NIST Election Common Standard Data Specification.  Some deviations
    /// from the standard exist.
    /// 
    /// This structure is considered an immutable input object and should not be changed
    /// through the course of an election, as it's hash representation is the basis for all
    /// other hash representations within an ElectionGuard election context.
    /// 
    /// <see href="https://www.electionguard.vote/overview/Glossary/#manifest">The ElectionGuard Documentation</see>
    /// <see href="https://developers.google.com/elections-data/reference/election">The Civics Common Standard Data Specification</see>
    /// </summary>
    public partial class Manifest : DisposableBase
    {
        /// <summary>
        /// Creates a <see cref="Manifest">Manifest</see> object from a <see href="https://www.rfc-editor.org/rfc/rfc8259.html#section-8.1">[RFC-8259]</see> UTF-8 encoded JSON string
        /// </summary>
        /// <param name="json">A UTF-8 Encoded JSON data string</param>
        public Manifest(string json)
        {
            var data = Language.EncodeNonAsciiCharacters(json);
            var status = NativeInterface.Manifest.FromJson(data, out Handle);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                ExceptionHandler.GetData(out var function, out var message, out var _);
                throw new ElectionGuardException($"Manifest Error Status: {status} function: {function} message: {message}");
            }
        }

        /// <summary> 
        /// Creates a `Manifest` object
        /// </summary>
        /// <param name="data">byte array of data describing the manifest</param>
        /// <param name="encoding">binary encoding for the data</param>
        public unsafe Manifest(byte[] data, BinarySerializationEncoding encoding)
        {
            fixed (byte* pointer = data)
            {
                var status = encoding == BinarySerializationEncoding.BSON
                    ? NativeInterface.Manifest.FromBson(pointer, (ulong)data.Length, out Handle)
                    : NativeInterface.Manifest.FromMsgPack(pointer, (ulong)data.Length, out Handle);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    ExceptionHandler.GetData(out var function, out var message, out var _);
                    throw new ElectionGuardException($"Manifest Error Binary Ctor: {status} function: {function} message: {message}");
                }
            }
        }

        /// <summary>
        /// Creates a `Manifest` object
        /// </summary>
        /// <param name="electionScopeId"></param>
        /// <param name="specVersion">election type</param>
        /// <param name="electionType">election type</param>
        /// <param name="startDate">start date for election</param>
        /// <param name="endDate">end data for the election</param>
        /// <param name="gpUnits">array of the `GeopoliticalUnit` for election</param>
        /// <param name="parties">array of the `Party` for election</param>
        /// <param name="candidates">array of the `Candidate` for election</param>
        /// <param name="contests">array of the `ContestDescription` for election</param>
        /// <param name="ballotStyles">array of the `BallotStyle` for election</param>
        public Manifest(
            string electionScopeId, string specVersion, ElectionType electionType,
            DateTime startDate, DateTime endDate,
            GeopoliticalUnit[] gpUnits, Party[] parties,
            Candidate[] candidates, ContestDescription[] contests,
            BallotStyle[] ballotStyles)
        {
            var gpUnitPointers = new IntPtr[gpUnits.Length];
            for (var i = 0; i < gpUnits.Length; i++)
            {
                gpUnitPointers[i] = gpUnits[i].Handle.Ptr;
                gpUnits[i].Dispose();
            }

            var partyPointers = new IntPtr[parties.Length];
            for (var i = 0; i < parties.Length; i++)
            {
                partyPointers[i] = parties[i].Handle.Ptr;
                parties[i].Dispose();
            }

            var candidatePointers = new IntPtr[candidates.Length];
            for (var i = 0; i < candidates.Length; i++)
            {
                candidatePointers[i] = candidates[i].Handle.Ptr;
                candidates[i].Dispose();
            }

            var contestPointers = new IntPtr[contests.Length];
            for (var i = 0; i < contests.Length; i++)
            {
                contestPointers[i] = contests[i].Handle.Ptr;
                contests[i].Dispose();
            }

            var ballotStylePointers = new IntPtr[ballotStyles.Length];
            for (var i = 0; i < ballotStyles.Length; i++)
            {
                ballotStylePointers[i] = ballotStyles[i].Handle.Ptr;
                ballotStyles[i].Dispose();
            }

            var status = NativeInterface.Manifest.New(
                electionScopeId, specVersion, electionType,
                (ulong)new DateTimeOffset(startDate).ToUnixTimeMilliseconds(),
                (ulong)new DateTimeOffset(endDate).ToUnixTimeMilliseconds(),
                gpUnitPointers, (ulong)gpUnitPointers.LongLength,
                partyPointers, (ulong)partyPointers.LongLength,
                candidatePointers, (ulong)candidatePointers.LongLength,
                contestPointers, (ulong)contestPointers.LongLength,
                ballotStylePointers, (ulong)ballotStylePointers.LongLength,
                 out Handle);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                ExceptionHandler.GetData(out var function, out var message, out var _);
                throw new ElectionGuardException($"Manifest Error Status: {status} function: {function} message: {message}");
            }
        }

        /// <summary>
        /// Creates a `Manifest` object
        /// </summary>
        /// <param name="electionScopeId"></param>
        /// <param name="electionType">election type</param>
        /// <param name="startDate">start date for election</param>
        /// <param name="endDate">end data for the election</param>
        /// <param name="gpUnits">array of the `GeopoliticalUnit` for election</param>
        /// <param name="parties">array of the `Party` for election</param>
        /// <param name="candidates">array of the `Candidate` for election</param>
        /// <param name="contests">array of the `ContestDescription` for election</param>
        /// <param name="ballotStyles">array of the `BallotStyle` for election</param>
        /// <param name="name">name of the election</param>
        /// <param name="contact">contact information for the election</param>
        public Manifest(
             string electionScopeId, ElectionType electionType,
             DateTime startDate, DateTime endDate,
             GeopoliticalUnit[] gpUnits, Party[] parties,
             Candidate[] candidates, ContestDescription[] contests,
             BallotStyle[] ballotStyles, InternationalizedText name, ContactInformation contact) : this(electionScopeId, "1.9", electionType,
             startDate, endDate, gpUnits, parties, candidates, contests, ballotStyles, name, contact)
        {
        }

        /// <summary>
        /// Creates a `Manifest` object
        /// </summary>
        /// <param name="electionScopeId"></param>
        /// <param name="specVersion">election type</param>
        /// <param name="electionType">election type</param>
        /// <param name="startDate">start date for election</param>
        /// <param name="endDate">end data for the election</param>
        /// <param name="gpUnits">array of the `GeopoliticalUnit` for election</param>
        /// <param name="parties">array of the `Party` for election</param>
        /// <param name="candidates">array of the `Candidate` for election</param>
        /// <param name="contests">array of the `ContestDescription` for election</param>
        /// <param name="ballotStyles">array of the `BallotStyle` for election</param>
        /// <param name="name">name of the election</param>
        /// <param name="contact">contact information for the election</param>
        public Manifest(
             string electionScopeId, string specVersion, ElectionType electionType,
             DateTime startDate, DateTime endDate,
             GeopoliticalUnit[] gpUnits, Party[] parties,
             Candidate[] candidates, ContestDescription[] contests,
             BallotStyle[] ballotStyles, InternationalizedText name, ContactInformation contact)
        {
            var gpUnitPointers = new IntPtr[gpUnits.Length];
            for (var i = 0; i < gpUnits.Length; i++)
            {
                gpUnitPointers[i] = gpUnits[i].Handle.Ptr;
                gpUnits[i].Dispose();
            }

            var partyPointers = new IntPtr[parties.Length];
            for (var i = 0; i < parties.Length; i++)
            {
                partyPointers[i] = parties[i].Handle.Ptr;
                parties[i].Dispose();
            }

            var candidatePointers = new IntPtr[candidates.Length];
            for (var i = 0; i < candidates.Length; i++)
            {
                candidatePointers[i] = candidates[i].Handle.Ptr;
                candidates[i].Dispose();
            }

            var contestPointers = new IntPtr[contests.Length];
            for (var i = 0; i < contests.Length; i++)
            {
                contestPointers[i] = contests[i].Handle.Ptr;
                contests[i].Dispose();
            }

            var ballotStylePointers = new IntPtr[ballotStyles.Length];
            for (var i = 0; i < ballotStyles.Length; i++)
            {
                ballotStylePointers[i] = ballotStyles[i].Handle.Ptr;
                ballotStyles[i].Dispose();
            }

            var status = NativeInterface.Manifest.New(
                electionScopeId, specVersion, electionType,
                (ulong)new DateTimeOffset(startDate).ToUnixTimeMilliseconds(),
                (ulong)new DateTimeOffset(endDate).ToUnixTimeMilliseconds(),
                gpUnitPointers, (ulong)gpUnitPointers.LongLength,
                partyPointers, (ulong)partyPointers.LongLength,
                candidatePointers, (ulong)candidatePointers.LongLength,
                contestPointers, (ulong)contestPointers.LongLength,
                ballotStylePointers, (ulong)ballotStylePointers.LongLength,
                name.Handle,
                contact.Handle,
                out Handle);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                ExceptionHandler.GetData(out var function, out var message, out var _);
                throw new ElectionGuardException($"Manifest Error Status: {status} function: {function} message: {message}");
            }
        }

        /// <Summary>
        /// Collection of geopolitical units for this election.
        /// </Summary>
        public GeopoliticalUnit GetGeopoliticalUnitAtIndex(ulong index)
        {
            var status = NativeInterface.Manifest.GetGeopoliticalUnitAtIndex(
                Handle, index, out var value);
            return status != Status.ELECTIONGUARD_STATUS_SUCCESS
                ? throw new ElectionGuardException($"Manifest Error GetGeopoliticalUnitAtIndex: {status}")
                : new GeopoliticalUnit(value);
        }

        /// <Summary>
        /// Collection of parties for this election.
        /// </Summary>
        public Party GetPartyAtIndex(ulong index)
        {
            var status = NativeInterface.Manifest.GetPartyAtIndex(
                Handle, index, out var value);
            return status != Status.ELECTIONGUARD_STATUS_SUCCESS
                ? throw new ElectionGuardException($"Manifest Error GetPartyAtIndex: {status}")
                : new Party(value);
        }

        /// <Summary>
        /// Collection of candidates for this election.
        /// </Summary>
        public Candidate GetCandidateAtIndex(ulong index)
        {
            var status = NativeInterface.Manifest.GetCandidateAtIndex(
                Handle, index, out var value);
            return status != Status.ELECTIONGUARD_STATUS_SUCCESS
                ? throw new ElectionGuardException($"Manifest Error GetCandidateAtIndex: {status}")
                : new Candidate(value);
        }

        /// <Summary>
        /// Collection of contests for this election.
        /// </Summary>
        public ContestDescription GetContestAtIndex(ulong index)
        {
            var status = NativeInterface.Manifest.GetContestAtIndex(
                Handle, index, out var value);
            return status != Status.ELECTIONGUARD_STATUS_SUCCESS
                ? throw new ElectionGuardException($"Manifest Error GetContestAtIndex: {status}")
                : new ContestDescription(value);
        }

        /// <Summary>
        /// Collection of ballot styles for this election.
        /// </Summary>
        public BallotStyle GetBallotStyleAtIndex(ulong index)
        {
            var status = NativeInterface.Manifest.GetBallotStyleAtIndex(
                Handle, index, out var value);
            return status != Status.ELECTIONGUARD_STATUS_SUCCESS
                ? throw new ElectionGuardException($"Manifest Error GetContestAtIndex: {status}")
                : new BallotStyle(value);
        }

        /// <Summary>
        /// A hash representation of the object
        /// </Summary>
        public ElementModQ CryptoHash()
        {
            var status = NativeInterface.Manifest.CryptoHash(
                    Handle, out var value);
            return status != Status.ELECTIONGUARD_STATUS_SUCCESS
                ? throw new ElectionGuardException($"CryptoHash Error Status: {status}")
                : new ElementModQ(value);
        }

        /// <Summary>
        /// Check whether the election manifest is valid and well-formed.
        /// </Summary>
        public bool IsValid()
        {
            var value = NativeInterface.Manifest.IsValid(Handle);
            return value;
        }

        /// <Summary>
        /// Export the ballot representation as JSON
        /// </Summary>
        public string ToJson()
        {
            var status = NativeInterface.Manifest.ToJson(
                Handle, out var pointer, out _);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                ExceptionHandler.GetData(out var function, out var message, out var _);
                throw new ElectionGuardException($"ToJson Error Status: {status} function: {function} message: {message}");
            }
            var json = pointer.PtrToStringUTF8();
            _ = NativeInterface.Memory.FreeIntPtr(pointer);
            return json;
        }

        /// <Summary>
        /// Export the ballot representation as ToBson
        /// </Summary>
        public byte[] ToBson()
        {

            var status = NativeInterface.Manifest.ToBson(
                Handle, out var data, out var size);

            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                ExceptionHandler.GetData(out var function, out var message, out var _);
                throw new ElectionGuardException($"Manifest Error ToBson: {status} function: {function} message: {message}");
            }

            if (size > int.MaxValue)
            {
                throw new ElectionGuardException($"Manifest Error ToBson: size is too big. Expected <= {int.MaxValue}, Actual: {size}.");
            }

            var byteArray = new byte[(int)size];
            Marshal.Copy(data, byteArray, 0, (int)size);
            _ = NativeInterface.Memory.DeleteIntPtr(data);
            return byteArray;
        }

        /// <Summary>
        /// Export the ballot representation as MsgPack
        /// </Summary>
        public byte[] ToMsgPack()
        {

            var status = NativeInterface.Manifest.ToMsgPack(
                Handle, out var data, out var size);

            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                ExceptionHandler.GetData(out var function, out var message, out var _);
                throw new ElectionGuardException($"Manifest Error ToMsgPack: {status} function: {function} message: {message}");
            }

            if (size > int.MaxValue)
            {
                throw new ElectionGuardException($"Manifest Error ToMsgPack: size is too big. Expected <= {int.MaxValue}, actual: {size}.");
            }

            var byteArray = new byte[(int)size];
            Marshal.Copy(data, byteArray, 0, (int)size);
            _ = NativeInterface.Memory.DeleteIntPtr(data);
            return byteArray;
        }
    }
}
