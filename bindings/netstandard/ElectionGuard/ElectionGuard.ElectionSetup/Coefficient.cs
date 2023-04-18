using System.Text.Json.Serialization;
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
    public EncryptionKeyPair KeyPair { get; private set; } = default!;

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
    public SchnorrProof Proof { get; private set; } = default!;

    /// <summary>
    /// Generate a random coefficient
    /// </summary>
    //[JsonConstructor]
    public Coefficient()
    {
        // var value = BigMath.RandQ();
        // KeyPair = ElGamalKeyPair.FromSecret(value);
        // var seed = BigMath.RandQ();
        // Proof = new(KeyPair, seed);
        Console.WriteLine("-------!!!!!!! WRONG CONSTRUCTOR Coefficient() !!!!!!!-------");

    }

    /// <summary>
    /// Generate a coefficient with a predetermined value
    /// </summary>
    public Coefficient(ElementModQ value)
    {
        var keyPair = ElGamalKeyPair.FromSecret(value);
        Proof = new SchnorrProof(keyPair);
        KeyPair = new EncryptionKeyPair(keyPair);
    }

    /// <summary>
    /// Generate a coefficient with a predetermined value and proof seed
    /// </summary>
    public Coefficient(ElementModQ value, ElementModQ seed)
    {
        var keyPair = ElGamalKeyPair.FromSecret(value);
        Proof = new SchnorrProof(keyPair, seed);
        KeyPair = new EncryptionKeyPair(keyPair);
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

        var keyPair = ElGamalKeyPair.FromSecret(value);
        if (!proof.PublicKey.Equals(keyPair.PublicKey))
        {
            throw new ArgumentException("Proof does not match key pair");
        }

        KeyPair = new EncryptionKeyPair(keyPair);
        Proof = new(proof);
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

        KeyPair = new EncryptionKeyPair(keyPair);
        Proof = new(proof);
    }

    public Coefficient(Coefficient that)

    {
        KeyPair = new EncryptionKeyPair(that.KeyPair);
        Proof = new SchnorrProof(that.Proof);
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
