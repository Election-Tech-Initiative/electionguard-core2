namespace ElectionGuard
{
    /// <summary>
    /// An object for caching election and encryption state.
    ///
    /// the encryption mediator composes ballots by querying the encryption device
    /// for a hash of its metadata and incremental timestamps/
    ///
    /// this is a convenience wrapper around the encrypt methods
    /// and may not be suitable for all use cases.
    /// </summary>
    public class EncryptionMediator : DisposableBase
    {
        internal NativeInterface.EncryptionMediator.EncryptionMediatorHandle Handle;

        /// <summary>
        /// Create an `EncryptionMediator` object
        /// </summary>
        /// <param name="manifest"></param>
        /// <param name="context"></param>
        /// <param name="device"></param>
        public EncryptionMediator(
            InternalManifest manifest,
            CiphertextElectionContext context,
            EncryptionDevice device)
        {
            if (manifest.ManifestHash.ToHex() != context.ManifestHash.ToHex())
            {
                Status.ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT.ThrowIfError();
            }

            var status = NativeInterface.EncryptionMediator.New(
                manifest.Handle, context.Handle, device.Handle, out Handle);
            status.ThrowIfError();
        }

        /// <summary>
        /// Encrypt the specified ballot using the cached election context.
        /// </summary>
        public CiphertextBallot Encrypt(
            PlaintextBallot plaintext, bool verifyProofs = false)
        {
            // TODO: default to usePrecomputedValues: false
            return Encrypt(plaintext, usePrecomputedValues: true, verifyProofs);
        }

        /// <summary>
        /// Encrypt the specified ballot using the cached election context.
        /// </summary>
        public CiphertextBallot Encrypt(
            PlaintextBallot plaintext, bool usePrecomputedValues, bool verifyProofs = false)
        {
            if (verifyProofs)
            {
                var status = NativeInterface.EncryptionMediator.EncryptAndVerify(
                    Handle, plaintext.Handle, usePrecomputedValues, out var ciphertext);
                status.ThrowIfError();
                return new CiphertextBallot(ciphertext);
            }
            else
            {
                var status = NativeInterface.EncryptionMediator.Encrypt(
                    Handle, plaintext.Handle, usePrecomputedValues, out var ciphertext);
                status.ThrowIfError();
                return new CiphertextBallot(ciphertext);
            }

        }

        /// <summary>
        /// Encrypt the specified ballot into its compact form using the cached election context.
        /// </summary>
        public CompactCiphertextBallot CompactEncrypt(
            PlaintextBallot plaintext, bool verifyProofs = false)
        {
            if (verifyProofs)
            {
                var status = NativeInterface.EncryptionMediator.CompactEncryptAndVerify(
                    Handle, plaintext.Handle, out var ciphertext);
                status.ThrowIfError();
                return new CompactCiphertextBallot(ciphertext);
            }
            else
            {
                var status = NativeInterface.EncryptionMediator.CompactEncrypt(
                    Handle, plaintext.Handle, out var ciphertext);
                status.ThrowIfError();
                return new CompactCiphertextBallot(ciphertext);
            }

        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        protected override void DisposeUnmanaged()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            base.DisposeUnmanaged();

            if (Handle == null || Handle.IsInvalid)
            {
                return;
            }

            Handle.Dispose();
            Handle = null;
        }
    }
}
