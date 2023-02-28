using System.ComponentModel.DataAnnotations;
using ElectionGuard.ElectionSetup.Extensions;
using ElectionGuard.UI.Lib.Extensions;
using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.ElectionSetup;

/// <summary>
/// A polynomial defined by coefficients
///
/// The 0-index coefficient is used for a secret key which can be
/// discovered by a quorum of n guardians corresponding to n coefficients.
/// </summary>
public class ElectionPolynomial : DisposableBase
{
    public ElectionPolynomial(List<Coefficient> coefficients)
    {
        Coefficients = coefficients;
    }

    public List<Coefficient> Coefficients { get; set; }

    /// <summary>
    /// Access the list of public keys generated from secret coefficient
    /// </summary>
    public List<ElementModP> GetCommitments()
    {
        return Coefficients
            .Select(i => i.Commitment)
            .ToList();
    }

    /// <summary>
    /// Access the list of proof of possession of the private key for the secret coefficient
    /// </summary>
    public List<SchnorrProof> GetProofs()
    {
        return Coefficients
            .Select(i => i.Proof)
            .ToList();
    }

    /// <summary>
    /// Generates a polynomial for sharing election keys
    /// </summary>
    /// <param name="numberOfCoefficients">Number of coefficients of polynomial</param>
    /// <param name="nonce">An optional nonce parameter that may be provided (useful for testing).</param>
    /// <returns>Polynomial used to share election keys</returns>
    /// <remarks>
    /// What is a coefficient?
    /// </remarks>
    public static ElectionPolynomial GeneratePolynomial(
        [Range(1, int.MaxValue)] int numberOfCoefficients,
        ElementModQ? nonce = null)
    {
        List<Coefficient> coefficients = new();

        for (var i = 0; i < numberOfCoefficients; i++)
        {
            coefficients.Add(GenerateCoefficient(nonce, i));
        }

        return new(coefficients);
    }

    private static Coefficient GenerateCoefficient(ElementModQ? nonce, int i)
    {
        var secretKey = GenerateSecretKey(nonce, i);
        var keypair = ElGamalKeyPair.FromSecret(secretKey);
        var seed = BigMath.RandQ();
        SchnorrProof proof = new(keypair, seed);

        return new(keypair, proof);
    }

    /// <summary>
    /// Force a random value to be generated for production purposes.
    /// In a production environment, nonce should be null and a random value will be generated.
    /// </summary>
    private static ElementModQ GenerateSecretKey(ElementModQ? nonce, int index)
    {
        if (nonce == null)
        {
            return BigMath.RandQ();
        }

        return BigMath.AddModQ(nonce, (ulong)index);
    }

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();

        Coefficients.Dispose();
    }
}
