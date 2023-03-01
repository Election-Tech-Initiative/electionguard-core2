using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.ElectionSetup;

public class EncryptionKeyPair : DisposableBase
{
    public ElementModP PublicKey { get; set; }

    public ElementModQ SecretKey { get; set; }

    public EncryptionKeyPair()
    {
        PublicKey = new ElementModP(0);
        SecretKey = new ElementModQ(0);
    }
    public EncryptionKeyPair(ElementModQ secretKey, ElementModP publicKey)
    {
        PublicKey = publicKey;
        SecretKey = secretKey;
    }

    public static implicit operator ElGamalKeyPair(EncryptionKeyPair data)
    {
        return ElGamalKeyPair.FromPair(data.SecretKey, data.PublicKey);
    }
    public static implicit operator EncryptionKeyPair(ElGamalKeyPair data)
    {
        return new ElGamalKeyPair(data.SecretKey, data.PublicKey);
    }
    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();

        PublicKey?.Dispose();
        SecretKey?.Dispose();
    }
}



/// <summary>
/// A tuple of election key pair, proof and polynomial
/// </summary>
public class ElectionKeyPair : DisposableBase
{
    public ElectionKeyPair()
    {
        OwnerId = string.Empty;
        SequenceOrder = 0;
        KeyPair = new EncryptionKeyPair();
        Polynomial = ElectionPolynomial.GeneratePolynomial(3);
    }

    public ElectionKeyPair(string ownerId, ulong sequenceOrder, ElGamalKeyPair keyPair, ElectionPolynomial polynomial)
    {
        OwnerId = ownerId;
        SequenceOrder = sequenceOrder;
        KeyPair = new(keyPair.SecretKey, keyPair.PublicKey);
        Polynomial = polynomial;
    }

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

    public ElectionPublicKey Share()
    {
        return new(
            OwnerId,
            SequenceOrder,
            KeyPair.PublicKey,
            Polynomial.GetCommitments(),
            Polynomial.GetProofs()
        );
    }

    public static ElectionKeyPair GenerateElectionKeyPair(
        string guardianId,
        ulong sequenceOrder,
        int quorum,
        ElementModQ? nonce)
    {
        var polynomial = ElectionPolynomial.GeneratePolynomial(quorum, nonce);
        var firstKeyPair = polynomial.Coefficients[0].KeyPair;

        return new(guardianId, sequenceOrder, firstKeyPair, polynomial);
    }

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();

        KeyPair.Dispose();
        Polynomial.Dispose();
    }
}
