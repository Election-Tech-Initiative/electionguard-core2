using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;
using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.ElectionSetup;

/// <summary>
/// A tuple of election key pair, proof and polynomial
/// </summary>
public class ElectionKeyPair : DisposableBase
{
    /// <summary>
    /// The id of the owner guardian
    /// </summary>
    public string OwnerId { get; set; }

    /// <summary>
    /// The sequence order of the owner guardian
    /// </summary>
    public ulong SequenceOrder { get; set; }

    /// <summary>
    /// The pair of public and private election keys for the guardian
    /// </summary>
    public EncryptionKeyPair KeyPair { get; set; }
    
    /// <summary>
    /// The secret polynomial for the guardian
    /// </summary>
    public ElectionPolynomial Polynomial { get; set; }

    public ElectionKeyPair() : this(string.Empty, 0, 3)
    {
    }

    /// <summary>
    /// Construct an Election Key Pair using a random secret key.
    /// The polynomial is generated using the random secret key.
    /// This override is used when the guardian generates the keypair internally.
    /// </summary>
    public ElectionKeyPair(
        string ownerId,
        ulong sequenceOrder,
        int quorum)
    {
        OwnerId = ownerId;
        SequenceOrder = sequenceOrder;
        using var randQ = BigMath.RandQ();
        using var pair = ElGamalKeyPair.FromSecret(randQ);
        KeyPair = new(pair.SecretKey, pair.PublicKey);
        Polynomial = new ElectionPolynomial(quorum, KeyPair);
    }

    /// <summary>
    /// Construct an Election Key Pair using the provided key pair.
    /// The polynomial is generated using the provided key pair.
    /// This override is used when the guardian generates the keypair externally.
    /// </summary>
    public ElectionKeyPair(
        string ownerId,
        ulong sequenceOrder,
        int quorum,
        ElGamalKeyPair keyPair)
    {
        OwnerId = ownerId;
        SequenceOrder = sequenceOrder;
        KeyPair = new(keyPair.SecretKey, keyPair.PublicKey);
        Polynomial = new ElectionPolynomial(quorum, keyPair);
    }

    /// <summary>
    /// Construct an Election Key Pair using the provided polynomial.
    /// The secret key is set to the zero-index coefficient per the spec.
    /// This override is used when the guardian generates the polynomial 
    /// externally but does not generate the keypair.
    /// </summary>
    public ElectionKeyPair(
        string ownerId,
        ulong sequenceOrder,
        ElectionPolynomial polynomial)
    {
        OwnerId = ownerId;
        SequenceOrder = sequenceOrder;
        Polynomial = polynomial;

        // set the secret key to the zero-index coefficient
        var ai_0 = polynomial.Coefficients[0].Value;
        KeyPair = ElGamalKeyPair.FromSecret(ai_0);
    }

    /// <summary>
    /// Construct an Election Key Pair using the provided key pair and polynomial.
    /// This override is used when the guardain generates the polynomial and keypair externally.
    /// </summary>
    public ElectionKeyPair(
        string ownerId,
        ulong sequenceOrder,
        ElGamalKeyPair keyPair,
        ElectionPolynomial polynomial)
    {
        OwnerId = ownerId;
        SequenceOrder = sequenceOrder;
        KeyPair = new(keyPair.SecretKey, keyPair.PublicKey);
        Polynomial = polynomial;

        // TODO: verify the polynomial is valid for the keypair
    }

    public ElectionPublicKey Share()
    {
        return new(
            OwnerId,
            SequenceOrder,
            KeyPair.PublicKey,
            Polynomial.Commitments,
            Polynomial.Proofs
        );
    }

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();

        KeyPair.Dispose();
        Polynomial.Dispose();
    }
}
