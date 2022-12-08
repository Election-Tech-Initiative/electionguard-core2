namespace ElectionGuard.ElectionSetup;
/// <summary>
/// A coefficient of an Election Polynomial
/// </summary>
public class Coefficient
{
    /// <summary>
    /// The key pair associated with the coefficient
    /// </summary>
    /// <value></value>
    public ElGamalKeyPair KeyPair { get; private set; }

    /// <summary>
    /// The secret coefficient `a_ij` 
    /// </summary>
    public ElementModQ Value => KeyPair.SecretKey;

    /// <summary>
    /// The public key `K_ij` generated from secret coefficient
    /// </summary>
    public ElementModP Commitment => KeyPair.PublicKey;

    /// <summary>
    /// A proof of possession of the private key for the secret coefficient
    /// </summary>
    public SchnorrProof Proof { get; private set; }

    public Coefficient(ElGamalKeyPair keyPair, SchnorrProof proof)
    {
        KeyPair = keyPair;
        Proof = proof;
    }
}
