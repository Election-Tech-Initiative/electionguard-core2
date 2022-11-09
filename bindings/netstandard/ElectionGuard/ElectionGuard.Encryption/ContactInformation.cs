using System;
using System.Runtime.InteropServices;

namespace ElectionGuard
{
    /// <Summary>
    /// For defining contact information about objects such as persons, boards of authorities, and organizations.
    ///
    /// Contact Information values are not used internally by ElectionGuard when encrypting ballots
    /// but are included for checking the validity of a supplied manifest.
    ///
    /// See: https://developers.google.com/elections-data/reference/contact-information
    /// </Summary>
    public class ContactInformation : DisposableBase
    {
        /// <Summary>
        /// Get the size of the address collection
        /// </Summary>
        public unsafe ulong AddressLineSize
        {
            get
            {
                var size = NativeInterface.ContactInformation.GetAddressLineSize(
                    Handle);
                return size;
            }
        }

        /// <Summary>
        /// Get the size of the email collection
        /// </Summary>
        public unsafe ulong EmailLineSize
        {
            get
            {
                var size = NativeInterface.ContactInformation.GetEmailLineSize(
                    Handle);
                return size;
            }
        }

        /// <Summary>
        /// Get the size of the phone collection
        /// </Summary>
        public unsafe ulong PhoneLineSeize
        {
            get
            {
                var size = NativeInterface.ContactInformation.GetPhoneLineSize(
                    Handle);
                return size;
            }
        }

        internal unsafe NativeInterface.ContactInformation.ContactInformationHandle Handle;

        internal unsafe ContactInformation(
            NativeInterface.ContactInformation.ContactInformationHandle handle)
        {
            Handle = handle;
        }

        /// <summary>
        /// Create a `ContactInformation` object
        /// </summary>
        /// <param name="name">name of the contact</param>
        public unsafe ContactInformation(string name)
        {
            var status = NativeInterface.ContactInformation.New(name, out Handle);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"ContactInformation Error Status: {status}");
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
        /// Associates an address with the contact.
        /// AddressLine needs to contain the lines that someone would
        /// enter into a web mapping service to find the address on a map.
        /// That is, the value of the field needs to geocode the contact location.
        /// </Summary>
        public unsafe string GetAddressLineAt(ulong index)
        {
            var status = NativeInterface.ContactInformation.GetAddressLineAtIndex(
                Handle, index, out IntPtr value);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"ContactInformation Error GetAddressLineAt: {status}");
            }
            var data = Marshal.PtrToStringAnsi(value);
            NativeInterface.Memory.FreeIntPtr(value);
            return data;
        }

        /// <Summary>
        /// Associates an email address with the contact.
        /// </Summary>
        public unsafe InternationalizedText GetEmailLineAt(ulong index)
        {
            var status = NativeInterface.ContactInformation.GetEmailLineAtIndex(
                Handle, index, out NativeInterface.InternationalizedText.InternationalizedTextHandle value);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"ContactInformation Error GetEmailLineAt: {status}");
            }
            return new InternationalizedText(value);
        }

        /// <Summary>
        /// Associates a phone number with the contact.
        /// </Summary>
        public unsafe InternationalizedText GetPhoneLineAt(ulong index)
        {
            var status = NativeInterface.ContactInformation.GetPhoneLineAtIndex(
                Handle, index, out NativeInterface.InternationalizedText.InternationalizedTextHandle value);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"ContactInformation Error GetPhoneLineAt: {status}");
            }
            return new InternationalizedText(value);
        }

        /// <Summary>
        /// A hash representation of the object
        /// </Summary>
        public unsafe ElementModQ CryptoHash()
        {
            var status = NativeInterface.ContactInformation.CryptoHash(
                Handle, out NativeInterface.ElementModQ.ElementModQHandle value);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"CryptoHash Error Status: {status}");
            }
            return new ElementModQ(value);
        }
    }
}