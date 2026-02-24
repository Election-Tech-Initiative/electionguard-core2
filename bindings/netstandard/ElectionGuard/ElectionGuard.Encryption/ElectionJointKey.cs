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


        public ElectionJointKey()
        {

        }

        public ElectionJointKey(
            ElementModP jointPublicKey, ElementModQ commitmentHash)
        {
            JointPublicKey = new ElementModP(jointPublicKey);
            CommitmentHash = new ElementModQ(commitmentHash);
        }

        public ElectionJointKey(ElectionJointKey other)
        {
            JointPublicKey = new ElementModP(other.JointPublicKey);
            CommitmentHash = new ElementModQ(other.CommitmentHash);
        }

        protected override void DisposeUnmanaged()
        {
            base.DisposeUnmanaged();

            JointPublicKey?.Dispose();
            CommitmentHash?.Dispose();
        }
    }
}
