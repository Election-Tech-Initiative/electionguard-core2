using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ElectionGuard
{
    /// <Summary>
    /// The ISO-639 language
    /// see: https://en.wikipedia.org/wiki/ISO_639
    /// see: https://developers.google.com/civics-data/reference/internationalized-text#language-string
    /// </Summary>
    public class Language : DisposableBase
    {
        /// <Summary>
        /// The value
        /// </Summary>
        public string Value
        {
            get
            {
                var status = NativeInterface.Language.GetValue(
                    Handle, out IntPtr value);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"Language Error Value: {status}");
                }
                var data = value.PtrToStringUTF8();
                NativeInterface.Memory.FreeIntPtr(value);
                return data;
            }
        }

        /// <Summary>
        /// Identifies the language
        /// </Summary>
        public string LanguageAbbreviation
        {
            get
            {
                var status = NativeInterface.Language.GetLanguage(
                    Handle, out IntPtr value);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"Language Error LanguageAbbreviation: {status}");
                }
                var data = Marshal.PtrToStringAnsi(value);
                NativeInterface.Memory.FreeIntPtr(value);
                return data;
            }
        }

        internal NativeInterface.Language.LanguageHandle Handle;

        internal Language(
            NativeInterface.Language.LanguageHandle handle)
        {
            Handle = handle;
        }

        /// <summary>
        /// Create a new `Language` object
        /// </summary>
        /// <param name="value">value to represent language</param>
        /// <param name="language">string with language info</param>
        public Language(string value, string language)
        {
            var data = EncodeNonAsciiCharacters(value);
            var status = NativeInterface.Language.New(data, language, out Handle);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"Language Error Status: {status}");
            }
        }

        /// <summary>
        /// Temp function for handling accented latin characters for v1.0
        /// </summary>
        /// <param name="value">string to convert</param>
        /// <returns>string with replaced characters</returns>
        public static string EncodeNonAsciiCharacters(string value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in value)
            {
                if (c > 127)
                {
                    // This character is too big for ASCII
                    string encodedValue = "\\u" + ((int)c).ToString("x4");
                    sb.Append(encodedValue);
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
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
            var status = NativeInterface.Language.CryptoHash(
                Handle, out NativeInterface.ElementModQ.ElementModQHandle value);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"CryptoHash Error Status: {status}");
            }
            return new ElementModQ(value);
        }
    }
}
