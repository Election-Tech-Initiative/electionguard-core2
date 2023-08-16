using System.ComponentModel.DataAnnotations;
using ElectionGuard.ElectionSetup.Extensions;
using ElectionGuard.Proofs;
using ElectionGuard.Extensions;

namespace ElectionGuard.ElectionSetup;

/// <summary>
/// A polynomial defined by coefficients
///
/// The 0-index coefficient is used for a secret key which can be
/// discovered by a quorum of n guardians corresponding to n coefficients.
/// </summary>
public class ElectionPolynomial : DisposableBase
{
    /// <summary>
    /// A collection of coefficients corresponding to an exponential order of the polynomial
    /// the zero-index coefficient is used for a secret key.
    /// </summary>
    public List<Coefficient> Coefficients { get; init; }

    /// <summary>
    /// Access the list of public keys generated from secret coefficient.
    /// 𝐾𝑖,j = g^𝑎𝑖,j mod p in the spec 
    /// Section 3.2.2 Details of key generation (10)
    /// </summary>
    [JsonIgnore]
    public List<ElementModP> Commitments => Coefficients
            .Select(i => i.Commitment)
            .ToList();

    /// <summary>
    /// Access the list of proof of possession of the private key for the secret coefficient
    /// </summary>
    [JsonIgnore]
    public List<SchnorrProof> Proofs => Coefficients
            .Select(i => i.Proof)
            .ToList();

    /// <summary>
    /// Generates a polynomial for sharing election keys. 
    /// Each coefficient is an exponential order for polynomial and the guardian secret key is the 0-index coefficient.
    /// </summary>
    /// <param name="numberOfCoefficients">Number of coefficients of polynomial, typically the quorum count of guardians</param>
    public ElectionPolynomial(
        ulong sequenceOrder,
        [Range(1, int.MaxValue)] int numberOfCoefficients,
        ElementModQ parameterHash)
    {
        Coefficients = new List<Coefficient>();
        for (var i = 0; i < numberOfCoefficients; i++)
        {
            using var secret = BigMath.RandQ();
            Coefficients.Add(new Coefficient(sequenceOrder, i, parameterHash, secret));
        }
    }

    /// <summary>
    /// Generates a polynomial for sharing election keys using the provided secret key as the zero-index coefficient.
    /// Each coefficient is an exponential order for the polynomial and the guardian secret key is the 0-index coefficient.
    /// </summary>
    /// <param name="numberOfCoefficients">Number of coefficients of polynomial, typically the quorum count of guardians</param>
    public ElectionPolynomial(
        ulong sequenceOrder,
        [Range(1, int.MaxValue)] int numberOfCoefficients,
        ElementModQ parameterHash,
        ElementModQ secretKey)
    {
        Coefficients = new List<Coefficient>
        {
            new(sequenceOrder, 0, parameterHash, secretKey)
        };
        for (var i = 1; i < numberOfCoefficients; i++)
        {
            using var secret = BigMath.RandQ();
            Coefficients.Add(new Coefficient(sequenceOrder, i, parameterHash, secret));
        }
    }

    /// <summary>
    /// Generates a polynomial for sharing election keys using the provided key pair as the zero-index coefficient.
    /// Each coefficient is an exponential order for the polynomial and the guardian secret key is the 0-index coefficient.
    /// </summary>
    /// <param name="numberOfCoefficients">Number of coefficients of polynomial, typically the quorum count of guardians</param>
    public ElectionPolynomial(
        ulong sequenceOrder,
        [Range(1, int.MaxValue)] int numberOfCoefficients,
        ElementModQ parameterHash,
        ElGamalKeyPair keyPair)
    {
        Coefficients = new List<Coefficient>
        {
            new(sequenceOrder, 0, parameterHash, keyPair.SecretKey)
            };
        for (var i = 1; i < numberOfCoefficients; i++)
        {
            using var secret = BigMath.RandQ();
            Coefficients.Add(new Coefficient(sequenceOrder, i, parameterHash, secret));
        }
    }

    /// <summary>
    /// Generates a polynomial for sharing election keys using the provided key pair as the zero-index coefficient.
    /// Each coefficient is an exponential order for the polynomial and the guardian secret key is the 0-index coefficient.
    /// </summary>
    /// <param name="numberOfCoefficients">Number of coefficients of polynomial, typically the quorum count of guardians</param>
    public ElectionPolynomial(
        ulong sequenceOrder,
        [Range(1, int.MaxValue)] int numberOfCoefficients,
        ElementModQ parameterHash,
        ElGamalKeyPair keyPair,
        Random random)
    {
        Coefficients = new List<Coefficient>
        {
            new(sequenceOrder, 0, parameterHash, keyPair.SecretKey)
        };
        for (var i = 1; i < numberOfCoefficients; i++)
        {
            using var secret = random.NextElementModQ();
            using var seed = random.NextElementModQ();
            Coefficients.Add(new Coefficient(sequenceOrder, i, parameterHash, secret, seed));
        }
    }

    /// <summary>
    /// Generates a polynomial for sharing election keys.
    /// Each coefficient is an exponential order for the polynomial and the guardian secret key is the 0-index coefficient.
    /// </summary>
    [JsonConstructor]
    public ElectionPolynomial(List<Coefficient> coefficients)
    {
        Coefficients = coefficients.Select(i => new Coefficient(i)).ToList();
    }

    /// <summary>
    /// Generates a polynomial for sharing election keys.
    /// Each coefficient is an exponential order for the polynomial and the guardian secret key is the 0-index coefficient.
    /// </summary>
    public ElectionPolynomial(ElectionPolynomial other)
    {
        Coefficients = new List<Coefficient>();
        for (var i = 0; i < other.Coefficients.Count; i++)
        {
            Coefficients.Add(new Coefficient(other.Coefficients[i]));
        }
    }

    /// <summary>
    /// Compute the single coordinate value of the polynomial at a given degree for use in sharing election keys.
    /// </summary>
    /// <param name="degree">The exponential degree of the polynomial (usually the sequence order)</param>
    public ElementModQ ComputeCoordinate(ulong degree)
    {
        using var degreeQ = new ElementModQ(degree);
        return ComputeCoordinate(degreeQ);
    }

    /// <summary>
    /// Compute the single coordinate value of the polynomial at a given degree for use in sharing election keys.
    /// </summary>
    /// <param name="degree">The exponential degree of the polynomial (usually the sequence order)</param>
    public ElementModQ ComputeCoordinate(ElementModQ degree)
    {
        var computedValue = new ElementModQ(Constants.ZERO_MOD_Q); // start at 0 mod q.

        foreach (var (coefficient, index) in Coefficients.WithIndex())
        {
            using var exponent = BigMath.PowModQ(degree, index);
            using var factor = BigMath.MultModQ(coefficient.Value, exponent);

            computedValue = BigMath.AddModQ(computedValue, factor);
        }

        return computedValue;
    }

    /// <summary>
    /// Verify the single coordinate value for a given degree is on the polynomial.
    /// </summary>
    /// <param name="degree">The exponential degree of the polynomial (usually the sequence order)</param>
    /// <param name="coordinate">The coordinate value of the polynomial at the given degree</param>
    public bool VerifyCoordinate(ulong degree, ElementModQ coordinate)
    {
        using var degreeQ = new ElementModQ(degree);
        return VerifyCoordinate(degreeQ, coordinate);
    }

    /// <summary>
    /// Verify the single coordinate value for a given degree is on the polynomial.
    /// </summary>
    /// <param name="degree">The exponential degree of the polynomial (usually the sequence order)</param>
    /// <param name="coordinate">The coordinate value of the polynomial at the given degree</param>
    public bool VerifyCoordinate(ElementModQ degree, ElementModQ coordinate)
    {
        return VerifyCoordinate(degree, coordinate, Coefficients.Select(i => i.Commitment).ToList());
    }

    /// <summary>
    /// Verify the single coordinate value for a given degree is on the polynomial.
    /// </summary>
    /// <param name="degree">The exponential degree of the polynomial (usually the sequence order)</param>
    /// <param name="coordinate">The coordinate value of the polynomial at the given degree</param>
    /// <param name="commitments">The commitments of the coefficients of the polynomial</param>
    public static bool VerifyCoordinate(
        ulong degree, ElementModQ coordinate, List<ElementModP> commitments)
    {
        using var calculated = new ElementModP(Constants.ONE_MOD_P); // start at 1 mod p.
        foreach (var (commitment, index) in commitments.WithIndex())
        {
            using var exponent = BigMath.PowModP(degree, index);
            using var factor = BigMath.PowModP(commitment, exponent);
            _ = calculated.MultModP(factor);
        }

        using var value = BigMath.GPowP(coordinate);
        return value.Equals(calculated);

    }

    /// <summary>
    /// Verify the single coordinate value for a given degree is on the polynomial.
    /// </summary>
    /// <param name="degree">The exponential degree of the polynomial (usually the sequence order)</param>
    /// <param name="coordinate">The coordinate value of the polynomial at the given degree</param>
    /// <param name="commitments">The commitments of the coefficients of the polynomial</param>
    public static bool VerifyCoordinate(
        ElementModQ degree, ElementModQ coordinate, List<ElementModP> commitments)
    {
        using var calculated = new ElementModP(Constants.ONE_MOD_P); // start at 1 mod p.
        foreach (var (commitment, index) in commitments.WithIndex())
        {
            using var exponent = BigMath.PowModP(degree, index);
            using var factor = BigMath.PowModP(commitment, exponent);
            _ = calculated.MultModP(factor);
        }

        using var value = BigMath.GPowP(coordinate);
        return value.Equals(calculated);
    }

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();
        Coefficients.Dispose();
    }
}
