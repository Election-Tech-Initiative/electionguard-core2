using ElectionGuard.Proofs;

namespace ElectionGuard.ElectionSetup;

/// <summary>
/// A coefficient of an Election Polynomial
/// </summary>
public class Coefficient : DisposableBase
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

    /// <summary>
    /// Generate a random coefficient
    /// </summary>
    public Coefficient()
    {
        using var value = BigMath.RandQ();
        KeyPair = ElGamalKeyPair.FromSecret(value);
        using var seed = BigMath.RandQ();
        Proof = new(KeyPair, seed);
    }

    /// <summary>
    /// Generate a coefficient with a predetermined value
    /// </summary>
    public Coefficient(ElementModQ value)
    {
        KeyPair = ElGamalKeyPair.FromSecret(value);
        Proof = new SchnorrProof(KeyPair);
    }

    /// <summary>
    /// Generate a coefficient with a predetermined value and proof seed
    /// </summary>
    public Coefficient(ElementModQ value, ElementModQ seed)
    {
        KeyPair = ElGamalKeyPair.FromSecret(value);
        Proof = new SchnorrProof(KeyPair, seed);
    }

    /// <summary>
    /// Generate a coefficient with a predetermined value and proof
    /// </summary>
    public Coefficient(ElementModQ value, SchnorrProof proof)
    {
        if (!proof.IsValid())
        {
            throw new ArgumentException("Invalid proof");
        }

        using var keyPair = ElGamalKeyPair.FromSecret(value);
        if (!proof.PublicKey.Equals(keyPair.PublicKey))
        {
            throw new ArgumentException("Proof does not match key pair");
        }

        KeyPair = keyPair;
        Proof = proof;
    }

    public Coefficient(ElGamalKeyPair keyPair, SchnorrProof proof)
    {
        if (!proof.IsValid())
        {
            throw new ArgumentException("Invalid proof");
        }

        if (!proof.PublicKey.Equals(keyPair.PublicKey))
        {
            throw new ArgumentException("Proof does not match key pair");
        }

        KeyPair = keyPair;
        Proof = proof;
    }

    public bool IsValid()
    {
        return Proof.IsValid() && Proof.PublicKey.Equals(KeyPair.PublicKey);
    }

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();

        KeyPair?.Dispose();
        Proof?.Dispose();
    }
}
