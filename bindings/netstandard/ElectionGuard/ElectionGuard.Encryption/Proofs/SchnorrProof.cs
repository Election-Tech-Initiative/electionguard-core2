using Newtonsoft.Json;

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

        [JsonConstructor]
        public SchnorrProof(
            ElementModP publicKey,
            ElementModP commitment,
            ElementModQ challenge,
            ElementModQ response)
        {
            PublicKey = new ElementModP(publicKey);
            Commitment = new ElementModP(commitment);
            Challenge = new ElementModQ(challenge);
            Response = new ElementModQ(response);
        }

        /// <summary>
        /// Create a new instance of a Schnorr proof using the provided secret and a random seed.
        /// </summary>
        public SchnorrProof(ElementModQ secretKey)
        {
            using (var keyPair = ElGamalKeyPair.FromSecret(secretKey))
            using (var seed = BigMath.RandQ())
            {
                PublicKey = new ElementModP(keyPair.PublicKey);
                Commitment = BigMath.GPowP(seed);
                Challenge = Hash.HashElems(PublicKey, Commitment);
                Response = BigMath.APlusBMulCModQ(seed, keyPair.SecretKey, Challenge);
            }
        }

        /// <summary>
        /// Create a new instance of a Schnorr proof using the provided key pair and a random seed.
        /// </summary>
        public SchnorrProof(ElGamalKeyPair keyPair)
        {
            using (var seed = BigMath.RandQ())
            {
                PublicKey = new ElementModP(keyPair.PublicKey);
                Commitment = BigMath.GPowP(seed);
                Challenge = Hash.HashElems(PublicKey, Commitment);
                Response = BigMath.APlusBMulCModQ(seed, keyPair.SecretKey, Challenge);
            }
        }

        /// <summary>
        /// Create a new instance of a Schnorr proof using the provided secret and seed.
        /// </summary>
        public SchnorrProof(ElementModQ secretKey, ElementModQ seed)
        {
            using (var keyPair = ElGamalKeyPair.FromSecret(secretKey))
            {

                PublicKey = new ElementModP(keyPair.PublicKey);
                Commitment = BigMath.GPowP(seed);
                Challenge = Hash.HashElems(PublicKey, Commitment);
                Response = BigMath.APlusBMulCModQ(seed, keyPair.SecretKey, Challenge);
            }
        }

        /// <summary>
        /// Create a new instance of a Schnorr proof using the provided key pair and seed.
        /// </summary>
        public SchnorrProof(ElGamalKeyPair keyPair, ElementModQ seed)
        {
            PublicKey = new ElementModP(keyPair.PublicKey);
            Commitment = BigMath.GPowP(seed);
            Challenge = Hash.HashElems(PublicKey, Commitment);
            Response = BigMath.APlusBMulCModQ(seed, keyPair.SecretKey, Challenge);
        }

        public SchnorrProof(SchnorrProof other)
        {
            PublicKey = new ElementModP(other.PublicKey);
            Commitment = new ElementModP(other.Commitment);
            Challenge = new ElementModQ(other.Challenge);
            Response = new ElementModQ(other.Response);
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

#pragma warning disable IDE0063 // Use simple 'using' statement. Need to support Net Standard 2.0, which doesn't have this.
            using (var c = Hash.HashElems(k, h))
            using (var gp = BigMath.GPowP(u))
            using (var pp = BigMath.PowModP(k, c))
            using (var mp = BigMath.MultModP(h, pp))
#pragma warning restore IDE0063
            {

                var validChallenge = c.Equals(Challenge);
                var validProof = gp.Equals(mp);

                var success = validPublicKey && inBoundsH && inBoundsU && validChallenge && validProof;
                if (success is false)
                {
                    #region commented Code
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
                    #endregion
                }
                return success;
            }
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
