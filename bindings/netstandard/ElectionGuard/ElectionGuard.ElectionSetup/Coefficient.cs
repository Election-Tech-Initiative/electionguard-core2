using ElectionGuard.Proofs;

namespace ElectionGuard.ElectionSetup;

/// <summary>
/// A coefficient of an Election Polynomial
/// </summary>
public class Coefficient : DisposableBase
{
    /// <summary>
    /// The offset of the secret value in the polynomial (i in the spec)
    /// </summary>
    public ulong Offset { get; set; }

    /// <summary>
    /// The index of the proof corresponding to the coefficient (j in the spec)
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// The key pair associated with the coefficient
    /// </summary>
    /// <value></value>
    public ElGamalKeyPair KeyPair { get; private set; } = default!;

    /// <summary>
    /// The secret coefficient `a_ij` 
    /// </summary>
    [JsonIgnore]
    public ElementModQ Value => KeyPair.SecretKey;

    /// <summary>
    /// The public key `K_ij` generated from secret coefficient
    /// </summary>
    [JsonIgnore]
    public ElementModP Commitment => KeyPair.PublicKey;

    /// <summary>
    /// A proof of possession of the private key for the secret coefficient
    /// </summary>
    public SchnorrProof Proof { get; private set; } = default!;

    /// <summary>
    /// Generate a coefficient with a predetermined secret
    /// </summary>
    public Coefficient(
        ulong offset,
        int index,
        ElementModQ parameterHash,
        ElementModQ secret)
    {
        Offset = offset;
        Index = index;
        var keyPair = ElGamalKeyPair.FromSecret(secret);
        Proof = new SchnorrProof(offset, index, parameterHash, keyPair);
        KeyPair = new ElGamalKeyPair(keyPair);
    }

    /// <summary>
    /// Generate a coefficient with a predetermined secret and proof seed
    /// </summary>
    public Coefficient(
        ulong offset,
        int index,
        ElementModQ parameterHash,
        ElementModQ secret, ElementModQ seed)
    {
        Offset = offset;
        Index = index;
        var keyPair = ElGamalKeyPair.FromSecret(secret);
        Proof = new SchnorrProof(offset, index, parameterHash, keyPair, seed);
        KeyPair = new ElGamalKeyPair(keyPair);
    }

    /// <summary>
    /// Generate a coefficient with a predetermined secret and proof
    /// </summary>
    public Coefficient(
        ulong offset,
        int index,
        ElementModQ parameterHash,
        ElementModQ secret, SchnorrProof proof)
    {
        if (!proof.IsValid(offset, index, parameterHash).Success)
        {
            throw new ArgumentException("Invalid proof");
        }

        var keyPair = ElGamalKeyPair.FromSecret(secret);
        if (!proof.PublicKey.Equals(keyPair.PublicKey))
        {
            throw new ArgumentException("Proof does not match key pair");
        }

        Offset = offset;
        Index = index;
        KeyPair = new ElGamalKeyPair(keyPair);
        Proof = new(proof);
    }

    [JsonConstructor]
    public Coefficient(
        ulong offset,
        int index,
        ElGamalKeyPair keyPair, SchnorrProof proof)
    {
        // Dos not check the proof for validity when serializing from json
        if (!proof.PublicKey.Equals(keyPair.PublicKey))
        {
            throw new ArgumentException("Proof does not match key pair");
        }
        Offset = offset;
        Index = index;
        KeyPair = new ElGamalKeyPair(keyPair);
        Proof = new(proof);
    }

    public Coefficient(Coefficient other)

    {
        KeyPair = new ElGamalKeyPair(other.KeyPair);
        Proof = new SchnorrProof(other.Proof);
    }

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();

        KeyPair?.Dispose();
        Proof?.Dispose();
    }
}
