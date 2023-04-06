using System.Runtime.InteropServices;

namespace ElectionGuard
{
    public class Nonces : DisposableBase
    {
        internal External.NonceSequenceHandle Handle;

        internal Nonces(External.NonceSequenceHandle handle)
        {
            Handle = handle;
        }

        /// <summary>
        /// Create a new NonceSequence
        /// </summary>
        public Nonces(ElementModQ seed)
        {
            var status = External.New(seed.Handle, out Handle);
            status.ThrowIfError();
        }

        public Nonces(ElementModQ seed, ElementModP header)
        {
            var status = External.New(seed.Handle, header.Handle, out Handle);
            status.ThrowIfError();
        }

        public Nonces(ElementModQ seed, ElementModQ header)
        {
            var status = External.New(seed.Handle, header.Handle, out Handle);
            status.ThrowIfError();
        }

        /// <summary>
        /// Create a new NonceSequence
        /// </summary>
        public Nonces(ElementModQ seed, string header)
        {
            var status = External.New(seed.Handle, header, out Handle);
            status.ThrowIfError();
        }

        /// <summary>
        /// Get a nonce value at a specific index
        /// </summary>
        public ElementModQ Get(ulong index)
        {
            var status = External.Get(Handle, index, out var value);
            status.ThrowIfError();

            return value.IsInvalid ? null : new ElementModQ(value);
        }

        /// <summary>
        /// Get a nonce value at a specific index
        /// </summary>
        public ElementModQ Get(ulong index, string header)
        {
            var status = External.Get(Handle, index, header, out var value);
            status.ThrowIfError();

            return value.IsInvalid ? null : new ElementModQ(value);
        }

        /// <summary>
        /// Get a the next nonce value
        /// </summary>
        public ElementModQ Next()
        {
            var status = External.Next(Handle, out var value);
            status.ThrowIfError();

            return value.IsInvalid ? null : new ElementModQ(value);
        }

        internal static unsafe class External
        {
            internal struct NonceSequenceType { };

            internal class NonceSequenceHandle
                : ElectionGuardSafeHandle<NonceSequenceType>
            {
                protected override bool Free()
                {
                    if (IsClosed)
                    {
                        return true;
                    }

                    var status = External.Free(TypedPtr);
                    return status != Status.ELECTIONGUARD_STATUS_SUCCESS
                        ? throw new ElectionGuardException($"NonceSequence Error Free: {status}", status)
                        : true;
                }
            }

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_nonce_sequence_new",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status New(
                NativeInterface.ElementModQ.ElementModQHandle seed,
                out NonceSequenceHandle handle
            );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_nonce_sequence_new_with_p_header",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status New(
                NativeInterface.ElementModQ.ElementModQHandle seed,
                NativeInterface.ElementModP.ElementModPHandle header,
                out NonceSequenceHandle handle
            );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_nonce_sequence_new_with_q_header",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status New(
                NativeInterface.ElementModQ.ElementModQHandle seed,
                NativeInterface.ElementModQ.ElementModQHandle header,
                out NonceSequenceHandle handle
            );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_nonce_sequence_new_with_string_header",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status New(
                NativeInterface.ElementModQ.ElementModQHandle seed,
                [MarshalAs(UnmanagedType.LPStr)] string header,
                out NonceSequenceHandle handle
            );

            [DllImport(NativeInterface.DllName, EntryPoint = "eg_nonce_sequence_free",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Free(NonceSequenceType* handle);

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_nonce_sequence_get_item",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status Get(
                NonceSequenceHandle handle,
                ulong item,
                out NativeInterface.ElementModQ.ElementModQHandle next
            );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_nonce_sequence_get_item_with_header",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status Get(
                NonceSequenceHandle handle,
                ulong item,
                [MarshalAs(UnmanagedType.LPStr)] string header,
                out NativeInterface.ElementModQ.ElementModQHandle next
            );

            // [DllImport(
            //     NativeInterface.DllName,
            //     EntryPoint = "eg_nonce_sequence_get_items",
            //     CallingConvention = CallingConvention.Cdecl,
            //     SetLastError = true)]
            // internal static extern Status Get(
            //     NonceSequenceHandle handle,
            //     ulong start, ulong count,
            //     out NativeInterface.ElementModQ.ElementModQHandle next
            // );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_nonce_sequence_next",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status Next(
                NonceSequenceHandle handle,
                out NativeInterface.ElementModQ.ElementModQHandle next
            );
        }
    }
}
