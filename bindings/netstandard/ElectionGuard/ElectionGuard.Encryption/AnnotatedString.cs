using System;
using System.Runtime.InteropServices;

namespace ElectionGuard
{
    /// <Summary>
    /// Use this as a type for character strings.
    /// See: https://developers.google.com/elections-data/reference/annotated-string
    /// </Summary>
    public class AnnotatedString : DisposableBase
    {
        /// <Summary>
        /// An annotation of up to 16 characters that's associated with a character string.
        /// </Summary>
        public string Annotation
        {
            get
            {
                var status = NativeInterface.AnnotatedString.GetAnnotation(
                    Handle, out IntPtr value);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"AnnotatedString Error Annotation: {status}");
                }
                var data = Marshal.PtrToStringAnsi(value);
                NativeInterface.Memory.FreeIntPtr(value);
                return data;
            }
        }

        /// <Summary>
        /// The character string
        /// </Summary>
        public string Value
        {
            get
            {
                var status = NativeInterface.AnnotatedString.GetValue(
                    Handle, out IntPtr value);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"AnnotatedString Error Value: {status}");
                }
                var data = Marshal.PtrToStringAnsi(value);
                NativeInterface.Memory.FreeIntPtr(value);
                return data;
            }
        }

        internal NativeInterface.AnnotatedString.AnnotatedStringHandle Handle;

        internal AnnotatedString(
            NativeInterface.AnnotatedString.AnnotatedStringHandle handle)
        {
            Handle = handle;
        }

        /// <summary>
        /// Creating a new `AnnotatedString`
        /// </summary>
        /// <param name="annotation">annotation for new string</param>
        /// <param name="value">string value</param>
        public AnnotatedString(string annotation, string value)
        {
            var status = NativeInterface.AnnotatedString.New(annotation, value, out Handle);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"AnnotatedString Error Status: {status}");
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
            var status = NativeInterface.AnnotatedString.CryptoHash(
                Handle, out NativeInterface.ElementModQ.ElementModQHandle value);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"CryptoHash Error Status: {status}");
            }
            return new ElementModQ(value);
        }
    }
}