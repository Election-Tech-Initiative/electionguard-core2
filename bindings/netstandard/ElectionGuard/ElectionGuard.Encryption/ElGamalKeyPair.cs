
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
        public ElementModP PublicKey
        {
            get
            {
                var status = NativeInterface.ElGamalKeyPair.GetPublicKey(
                    Handle, out var value);
                status.ThrowIfError();
                if (value.IsInvalid)
                {
                    return null;
                }
                return new ElementModP(value);
            }
        }

        /// <Summary>
        /// The ElGamal Secret Key.
        /// </Summary>
        public ElementModQ SecretKey
        {
            get
            {
                var status = NativeInterface.ElGamalKeyPair.GetSecretKey(
                    Handle, out var value);
                status.ThrowIfError();
                if (value.IsInvalid)
                {
                    return null;
                }
                return new ElementModQ(value);
            }
        }

        internal NativeInterface.ElGamalKeyPair.ElGamalKeyPairHandle Handle;

        internal ElGamalKeyPair(NativeInterface.ElGamalKeyPair.ElGamalKeyPairHandle handle)
        {
            Handle = handle;
        }

        public ElGamalKeyPair(ElementModQ secretKey)
        {
            var status = NativeInterface.ElGamalKeyPair.New(
                secretKey.Handle, out Handle);
            status.ThrowIfError();
        }

        public ElGamalKeyPair(ElementModQ secretKey, ElementModP publicKey)
        {
            var status = NativeInterface.ElGamalKeyPair.New(
                secretKey.Handle, publicKey.Handle, out Handle);
            status.ThrowIfError();
        }

        /// <Summary>
        /// Make an elgamal key pair from a secret.
        /// </Summary>
        public static ElGamalKeyPair FromSecret(ElementModQ secretKey)
        {
            return new ElGamalKeyPair(secretKey);
        }

        /// <summary>
        /// Make an elgamal key pair from a secret and a public key.
        /// </summary>
        /// <param name="secretKey"></param>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        public static ElGamalKeyPair FromPair(ElementModQ secretKey, ElementModP publicKey)
        {
            return new ElGamalKeyPair(secretKey, publicKey);
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
