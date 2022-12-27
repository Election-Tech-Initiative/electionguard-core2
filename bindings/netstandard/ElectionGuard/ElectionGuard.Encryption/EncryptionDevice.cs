using System;
using System.Runtime.InteropServices;

namespace ElectionGuard
{
    /// <summary>
    /// Metadata for encryption device
    ///
    /// The encryption device is a stateful container that represents abstract hardware
    /// authorized to participate in a specific election.
    ///
    /// </summary>
    public class EncryptionDevice : DisposableBase
    {
        internal NativeInterface.EncryptionDevice.EncryptionDeviceHandle Handle;

        /// <summary>
        /// Create a new EncryptionDevice
        /// </summary>
        /// <param name="deviceUuid">a unique identifier tied to the device hardware</param>
        /// <param name="sessionUuid">a unique identifier tied to the runtime session</param>
        /// <param name="launchCode">a unique identifier tied to the election</param>
        /// <param name="location">an arbitrary string meaningful to the external system
        ///                        such as a friendly name, description, or some other value</param>
        public EncryptionDevice(
            ulong deviceUuid,
            ulong sessionUuid,
            ulong launchCode,
            string location)
        {
            var status = NativeInterface.EncryptionDevice.New(
                deviceUuid, sessionUuid, launchCode, location, out Handle);
            status.ThrowIfError();
        }

        /// <summary>
        /// Get a new hash value
        ///
        /// <return>An `ElementModQ`</return>
        /// </summary>
        public ElementModQ GetHash()
        {
            var status = NativeInterface.EncryptionDevice.GetHash(Handle, out NativeInterface.ElementModQ.ElementModQHandle value);
            status.ThrowIfError();
            return new ElementModQ(value);
        }

        /// <summary>
        /// Creates an <see cref="EncryptionDevice">EncryptionDevice</see> object from a <see href="https://www.rfc-editor.org/rfc/rfc8259.html#section-8.1">[RFC-8259]</see> UTF-8 encoded JSON string
        /// </summary>
        /// <param name="json">A UTF-8 Encoded JSON data string</param>
        public EncryptionDevice(string json)
        {
            var status = NativeInterface.EncryptionDevice.FromJson(json, out Handle);
            status.ThrowIfError();
        }
        /// <Summary>
        /// Export the encryption device representation as JSON
        /// </Summary>
        public string ToJson()
        {
            var status = NativeInterface.EncryptionDevice.ToJson(Handle, out IntPtr pointer, out _);
            status.ThrowIfError();
            var json = Marshal.PtrToStringAnsi(pointer);
            NativeInterface.Memory.FreeIntPtr(pointer);
            return json;
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
    }
}