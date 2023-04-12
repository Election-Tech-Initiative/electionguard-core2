namespace ElectionGuard.Proofs
{

    /// <summary>
    /// Representation of a Schnorr proof
    /// </summary>
    public class SchnorrProof : DisposableBase
    {
        /// <summary>
        /// k in the spec
        /// </summary>
        public ElementModP PublicKey { get; private set; }

        /// <summary>
        /// h in the spec
        /// </summary>
        public ElementModP Commitment { get; private set; }

        /// <summary>
        /// c in the spec
        /// </summary>
        public ElementModQ Challenge { get; private set; }

        /// <summary>
        /// u in the spec
        /// </summary>
        public ElementModQ Response { get; private set; }

        public ProofUsage Usage = ProofUsage.SecretValue;

        public SchnorrProof()
        {
            var randQ = BigMath.RandQ();
            var keyPair = ElGamalKeyPair.FromSecret(randQ);
            var seed = BigMath.RandQ();

            PublicKey = keyPair.PublicKey;
            Commitment = BigMath.GPowP(seed);
            Challenge = BigMath.HashElems(PublicKey, Commitment);
            Response = BigMath.APlusBMulCModQ(seed, keyPair.SecretKey, Challenge);

            keyPair.Dispose();
            randQ.Dispose();
            seed.Dispose();
        }

        /// <summary>
        /// Create a new instance of a Schnorr proof using the provided secret and a random seed.
        /// </summary>
        public SchnorrProof(ElementModQ secretKey)
        {
            var keyPair = ElGamalKeyPair.FromSecret(secretKey);
            var seed = BigMath.RandQ();

            PublicKey = keyPair.PublicKey;
            Commitment = BigMath.GPowP(seed);
            Challenge = BigMath.HashElems(PublicKey, Commitment);
            Response = BigMath.APlusBMulCModQ(seed, keyPair.SecretKey, Challenge);

            keyPair.Dispose();
            seed.Dispose();
        }

        /// <summary>
        /// Create a new instance of a Schnorr proof using the provided key pair and a random seed.
        /// </summary>
        public SchnorrProof(ElGamalKeyPair keyPair)
        {
            var seed = BigMath.RandQ();

            PublicKey = keyPair.PublicKey;
            Commitment = BigMath.GPowP(seed);
            Challenge = BigMath.HashElems(PublicKey, Commitment);
            Response = BigMath.APlusBMulCModQ(seed, keyPair.SecretKey, Challenge);

            seed.Dispose();
        }

        /// <summary>
        /// Create a new instance of a Schnorr proof using the provided secret and seed.
        /// </summary>
        public SchnorrProof(ElementModQ secretKey, ElementModQ seed)
        {
            var keyPair = ElGamalKeyPair.FromSecret(secretKey);

            PublicKey = keyPair.PublicKey;
            Commitment = BigMath.GPowP(seed);
            Challenge = BigMath.HashElems(PublicKey, Commitment);
            Response = BigMath.APlusBMulCModQ(seed, keyPair.SecretKey, Challenge);

            keyPair.Dispose();
        }

        /// <summary>
        /// Create a new instance of a Schnorr proof using the provided key pair and seed.
        /// </summary>
        public SchnorrProof(ElGamalKeyPair keyPair, ElementModQ seed)
        {
            PublicKey = keyPair.PublicKey;
            Commitment = BigMath.GPowP(seed);
            Challenge = BigMath.HashElems(PublicKey, Commitment);
            Response = BigMath.APlusBMulCModQ(seed, keyPair.SecretKey, Challenge);
        }

        /// <summary>
        /// Check validity of the `proof` for proving possession of the secret key corresponding to the public key
        /// </summary>     
        public bool IsValid()
        {
            var k = PublicKey;
            var h = Commitment;
            var u = Response;
            var validPublicKey = k.IsValidResidue();
            var inBoundsH = h.IsInBounds();
            var inBoundsU = u.IsInBounds();

            var c = BigMath.HashElems(k, h);
            var gp = BigMath.GPowP(u);
            var pp = BigMath.PowModP(k, c);
            var mp = BigMath.MultModP(h, pp);

            var validChallenge = c.Equals(Challenge);
            var validProof = gp.Equals(mp);

            var success = validPublicKey && inBoundsH && inBoundsU && validChallenge && validProof;
            if (success is false)
            {
                // TODO: result
                //log_warning(
                //    "found an invalid Schnorr proof: %s",
                //    str(
                //            {
                //    "in_bounds_h": in_bounds_h,
                //                "in_bounds_u": in_bounds_u,
                //                "valid_public_key": valid_public_key,
                //                "valid_challenge": valid_challenge,
                //                "valid_proof": valid_proof,
                //                "proof": self,
                //            }
                //        ),
                //    )
            }

            c.Dispose();
            gp.Dispose();
            pp.Dispose();
            mp.Dispose();

            return success;
        }

        protected override void DisposeUnmanaged()
        {
            base.DisposeUnmanaged();

            PublicKey.Dispose();
            Commitment.Dispose();
            Challenge.Dispose();
            Response.Dispose();
        }
    }
}
