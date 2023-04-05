namespace ElectionGuard
{
    /// <summary>
    /// The Election joint key
    /// </summary>
    public class ElectionJointKey : DisposableBase
    {
        /// <summary>
        /// The product of the guardian public keys
        /// K = ∏ ni=1 Ki mod p.
        /// </summary>
        public ElementModP JointPublicKey { get; set; }

        /// <summary>
        /// The hash of the commitments that the guardians make to each other
        /// H = H(K 1,0 , K 2,0 ... , K n,0 )
        /// </summary>
        public ElementModQ CommitmentHash { get; set; }

        protected override void DisposeUnmanaged()
        {
            base.DisposeUnmanaged();

            JointPublicKey?.Dispose();
            CommitmentHash?.Dispose();
        }
    }
}
