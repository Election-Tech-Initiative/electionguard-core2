using System;
using Microsoft.Win32.SafeHandles;

namespace ElectionGuard
{
    internal abstract class ElectionGuardSafeHandle
       : SafeHandleZeroOrMinusOneIsInvalid
    {
        // Objects constructed with the default constructor
        // own the context
        internal ElectionGuardSafeHandle()
            : base(ownsHandle: true)
        {

        }

        internal ElectionGuardSafeHandle(bool ownsHandle)
            : base(ownsHandle)
        {

        }

        // Objects constructed from a structure pointer
        // do not own the context
        internal ElectionGuardSafeHandle(
            IntPtr handle)
            : base(ownsHandle: false)
        {
            SetHandle(handle);
        }

        internal ElectionGuardSafeHandle(
            IntPtr handle, bool ownsHandle)
            : base(ownsHandle)
        {
            SetHandle(handle);
        }

        public IntPtr Ptr => handle;

        public virtual void ThrowIfInvalid()
        {
            if (IsInvalid)
            {
                throw new ElectionGuardException($"{GetType().Name} ERROR Invalid Handle");
            }
        }

        protected bool IsFreed;

        protected abstract bool Free();

        protected override bool ReleaseHandle()
        {
            try
            {
                var freed = Free();
                if (freed)
                {
                    IsFreed = true;
                    Close();
                }
                return freed;
            }
            catch (Exception ex)
            {
                throw new ElectionGuardException($"{GetType().Name} ERROR ReleaseHandle: {ex.Message}", ex);
            }
        }
    }

    internal abstract class ElectionGuardSafeHandle<T>
        : ElectionGuardSafeHandle
        where T : unmanaged
    {
        // Objects constructed with the default constructor
        // own the context
        internal ElectionGuardSafeHandle() : base()
        {

        }

        // Objects constructed from a structure pointer
        // do not own the context
        internal ElectionGuardSafeHandle(
            IntPtr handle) : base(handle)
        {

        }

        internal ElectionGuardSafeHandle(
            IntPtr handle, bool ownsHandle)
            : base(handle, ownsHandle)
        {

        }

        internal unsafe ElectionGuardSafeHandle(
            T* handle)
            : base(ownsHandle: false)
        {
            SetHandle((IntPtr)handle);
        }

        internal unsafe ElectionGuardSafeHandle(
            T* handle, bool ownsHandle)
            : base(ownsHandle)
        {
            SetHandle((IntPtr)handle);
        }

        public unsafe T* TypedPtr => (T*)handle;

        public override void ThrowIfInvalid()
        {
            if (IsInvalid)
            {
                throw new ElectionGuardException($"{nameof(T)} ERROR Invalid Handle");
            }
        }

        protected override bool ReleaseHandle()
        {
            try
            {
                var freed = Free();
                if (freed)
                {
                    IsFreed = true;
                    Close();
                }
                return freed;
            }
            catch (Exception ex)
            {
                throw new ElectionGuardException($"{nameof(T)} ERROR ReleaseHandle: {ex.Message}", ex);
            }
        }
    }
}
