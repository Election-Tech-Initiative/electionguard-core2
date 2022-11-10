using System;
using System.Runtime.InteropServices;

namespace ElectionGuard
{
    /// <Summary>
    /// Use this entity to describe a political party that can then be referenced from other entities.
    ///
    /// It is not required to define a party for ElectionGuard.
    ///
    /// See: https://developers.google.com/elections-data/reference/party
    /// </Summary>
    public class Party : DisposableBase
    {
        /// <Summary>
        /// Unique internal identifier that's used by other elements to reference this element.
        /// </Summary>
        public string ObjectId
        {
            get
            {
                var status = NativeInterface.Party.GetObjectId(
                    Handle, out IntPtr value);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"Party Error ObjectId: {status}");
                }
                var data = Marshal.PtrToStringAnsi(value);
                NativeInterface.Memory.FreeIntPtr(value);
                return data;
            }
        }

        /// <Summary>
        /// Abbreviation of the party
        /// </Summary>
        public string Abbreviation
        {
            get
            {
                var status = NativeInterface.Party.GetAbbreviation(
                    Handle, out IntPtr value);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"Party Error Abbreviation: {status}");
                }
                var data = Marshal.PtrToStringAnsi(value);
                NativeInterface.Memory.FreeIntPtr(value);
                return data;
            }
        }

        /// <Summary>
        /// Name of the party
        /// </Summary>
        public InternationalizedText Name
        {
            get
            {
                var status = NativeInterface.Party.GetName(
                    Handle, out NativeInterface.InternationalizedText.InternationalizedTextHandle value);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"Party Error Name: {status}");
                }
                return new InternationalizedText(value);
            }
        }

        /// <Summary>
        /// An optional color in hex
        /// </Summary>
        public string Color
        {
            get
            {
                var status = NativeInterface.Party.GetColor(
                    Handle, out IntPtr value);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"Party Error Color: {status}");
                }
                var data = Marshal.PtrToStringAnsi(value);
                NativeInterface.Memory.FreeIntPtr(value);
                return data;
            }
        }

        /// <Summary>
        /// An optional logo uri
        /// </Summary>
        public string LogoUri
        {
            get
            {
                var status = NativeInterface.Party.GetLogoUri(
                    Handle, out IntPtr value);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"Party Error LogoUri: {status}");
                }
                var data = Marshal.PtrToStringAnsi(value);
                NativeInterface.Memory.FreeIntPtr(value);
                return data;
            }
        }

        internal NativeInterface.Party.PartyHandle Handle;

        internal Party(
            NativeInterface.Party.PartyHandle handle)
        {
            Handle = handle;
        }

        /// <summary>
        /// Create a `Party` object
        /// </summary>
        /// <param name="objectId">string to identify the object</param>
        public Party(string objectId)
        {
            var status = NativeInterface.Party.New(objectId, out Handle);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"Party Error Status: {status}");
            }
        }

        /// <summary>
        /// Create a `Party` object
        /// </summary>
        /// <param name="objectId">string to identify the object</param>
        /// <param name="name">name of the party</param>
        /// <param name="abbreviation">abbreviation for the object</param>
        /// <param name="color">string for the name of the color used</param>
        /// <param name="logoUri">string for the uri for the logo</param>
        public Party(
            string objectId, InternationalizedText name,
            string abbreviation, string color, string logoUri)
        {
            var status = NativeInterface.Party.New(
                objectId, name.Handle, abbreviation, color ?? string.Empty, logoUri ?? string.Empty, out Handle);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"Party Error Status: {status}");
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
            var status = NativeInterface.Party.CryptoHash(
                Handle, out NativeInterface.ElementModQ.ElementModQHandle value);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"CryptoHash Error Status: {status}");
            }
            return new ElementModQ(value);
        }
    }
}