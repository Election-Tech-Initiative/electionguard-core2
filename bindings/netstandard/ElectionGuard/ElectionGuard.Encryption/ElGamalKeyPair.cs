namespace ElectionGuard
{
    /// <summary>
    /// An exponential ElGamal key pair
    /// </summary>
    public class ElGamalKeyPair : DisposableBase
    {
        /// <Summary>
        /// The ElGamal Public Key.
        /// </Summary>
        public unsafe ElementModP PublicKey
        {
            get
            {
                var status = NativeInterface.ElGamalKeyPair.GetPublicKey(
                    Handle, out NativeInterface.ElementModP.ElementModPHandle value);
                status.ThrowIfError();
                return new ElementModP(value);
            }
        }

        /// <Summary>
        /// The ElGamal Secret Key.
        /// </Summary>
        public unsafe ElementModQ SecretKey
        {
            get
            {
                var status = NativeInterface.ElGamalKeyPair.GetSecretKey(
                    Handle, out NativeInterface.ElementModQ.ElementModQHandle value);
                status.ThrowIfError();
                return new ElementModQ(value);
            }
        }

        internal unsafe NativeInterface.ElGamalKeyPair.ElGamalKeyPairHandle Handle;

        internal unsafe ElGamalKeyPair(NativeInterface.ElGamalKeyPair.ElGamalKeyPairHandle handle)
        {
            Handle = handle;
        }

        internal unsafe ElGamalKeyPair(ElementModQ secretKey)
        {
            var status = NativeInterface.ElGamalKeyPair.New(
                secretKey.Handle, out Handle);
            status.ThrowIfError();
        }

        /// <Summary>
        /// Make an elgamal key pair from a secret.
        /// </Summary>
        public static unsafe ElGamalKeyPair FromSecret(ElementModQ secretKey)
        {
            return new ElGamalKeyPair(secretKey);
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