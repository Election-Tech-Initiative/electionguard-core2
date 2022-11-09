using System;

namespace ElectionGuard
{
    using NativeContextConfig = NativeInterface.ContextConfiguration.ContextConfigurationHandle;


    /// <summary>
    /// Class to handle configuration settings for the ElectionContext
    /// </summary>
    public class ContextConfiguration : DisposableBase
    {
        internal unsafe NativeContextConfig Handle;
        unsafe internal ContextConfiguration(NativeContextConfig handle)
        {
            Handle = handle;
        }

        /// <summary>
        /// Determines if overvotes are allowed for the election.  This defaults to true
        /// </summary>
        public unsafe bool AllowedOverVotes
        {
            get
            {
                bool value = true;
                var status = NativeInterface.ContextConfiguration.GetAllowedOverVotes(
                    Handle, ref value);
                status.ThrowIfError();
                return value;
            }
        }
        /// <summary>
        /// Determines the maximum number of votes that an election can have, Defaults to 1000000
        /// </summary>
        public unsafe UInt64 MaxBallots
        {
            get
            {
                UInt64 value = 1;
                var status = NativeInterface.ContextConfiguration.GetMaxBallots(
                    Handle, ref value);
                status.ThrowIfError();
                return value;
            }
        }

        /// <summary>
        /// Parameterized constructor for the configuration class
        /// </summary>
        /// <param name="allowOverVotes">if overvotes are allowed</param>
        /// <param name="maxVotes">maximum number of votes</param>
        public unsafe ContextConfiguration(bool allowOverVotes, UInt64 maxVotes)
        {
            var status = NativeInterface.ContextConfiguration.Make(
                allowOverVotes, maxVotes, out Handle);
            status.ThrowIfError();
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


    }
}
