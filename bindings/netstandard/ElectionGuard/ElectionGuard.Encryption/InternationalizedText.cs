using System;

namespace ElectionGuard
{
    /// <Summary>
    /// Data entity used to represent multi-national text. Use when text on a ballot contains multi-national text.
    /// See: https://developers.google.com/elections-data/reference/internationalized-text
    /// </Summary>
    public class InternationalizedText : DisposableBase
    {
        /// <Summary>
        /// A string of possibly non-English text.
        /// </Summary>
        public ulong TextSize
        {
            get
            {
                var size = NativeInterface.InternationalizedText.GetTextSize(
                    Handle);
                return size;
            }
        }

        internal NativeInterface.InternationalizedText.InternationalizedTextHandle Handle;

        internal InternationalizedText(
            NativeInterface.InternationalizedText.InternationalizedTextHandle handle)
        {
            Handle = handle;
        }

        /// <summary>
        /// Create an `InternationalizedText` object
        /// </summary>
        /// <param name="text">array of text for languages</param>
        public InternationalizedText(Language[] text)
        {
            IntPtr[] nativeText = new IntPtr[text.Length];
            for (var i = 0; i < text.Length; i++)
            {
                nativeText[i] = text[i].Handle.Ptr;
                text[i].Dispose();
            }

            var status = NativeInterface.InternationalizedText.New(nativeText, (ulong)nativeText.Length, out Handle);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"InternationalizedText Error Status: {status}");
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

        /// <summary>
        /// Gets text from array of languages
        /// </summary>
        /// <param name="index">index to use to get `Language`</param>
        /// <returns>`Language` object</returns>
        public Language GetTextAt(ulong index)
        {
            var status = NativeInterface.InternationalizedText.GetTextAtIndex(
                Handle, index, out NativeInterface.Language.LanguageHandle value);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"InternationalizedText Error GetTextAt: {status}");
            }
            return new Language(value);
        }

        /// <Summary>
        /// A hash representation of the object
        /// </Summary>
        public ElementModQ CryptoHash()
        {
            var status = NativeInterface.InternationalizedText.CryptoHash(
                Handle, out NativeInterface.ElementModQ.ElementModQHandle value);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"CryptoHash Error Status: {status}");
            }
            return new ElementModQ(value);
        }
    }
}