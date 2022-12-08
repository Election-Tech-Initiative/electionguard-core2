namespace ElectionGuard.ElectionSetup;

/// <summary>
/// Representation of a Schnorr proof
/// </summary>
public class SchnorrProof
{
    /// <summary>
    /// k in the spec
    /// </summary>
    public ElementModP PublicKey { get; set; }

    /// <summary>
    /// h in the spec
    /// </summary>
    public ElementModP Commitment { get; set; }

    /// <summary>
    /// c in the spec
    /// </summary>
    public ElementModQ Challenge { get; set; }

    /// <summary>
    /// u in the spec
    /// </summary>
    public ElementModQ Response { get; set; }

    public ProofUsage Usage = ProofUsage.SecretValue;

    public bool IsValid()
    {
        // TODO: implement
        return true;
    }

    public SchnorrProof(ElGamalKeyPair keyPair) : this(keyPair, BigMath.RandQ()) { }

    /// <summary>
    /// Create a new instance of a Schnorr proof.
    /// </summary>
    /// <remarks>
    ///   Do not use this constructor directly, unless unit testing. The nonce should never be known to the caller.
    ///   Use the <see cref="SchnorrProof(ElGamalKeyPair)"/> constructor instead.
    /// </remarks>
    /// <param name="keyPair"></param>
    /// <param name="randomSeed"></param>
    public SchnorrProof(ElGamalKeyPair keyPair, ElementModQ randomSeed)
    {
        PublicKey = keyPair.PublicKey;
        Commitment = BigMath.GPowP(randomSeed);
        Challenge = BigMath.HashElems(PublicKey, Commitment);
        Response = BigMath.APlusBMulCModQ(randomSeed, keyPair.SecretKey, Challenge);
    }
}
