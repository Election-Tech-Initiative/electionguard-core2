namespace ElectionGuard.ElectionSetup.Tests;

public static class CoefficientExtensions
{
    /// <summary>
    /// Generate a Coefficient for testing purposes only.
    /// In a production environment, nonce should be null and a random value will be generated.
    /// </summary>
    public static Coefficient GenerateCoefficient(ulong offset, int index, ElementModQ parameterHash, ElementModQ seed)
    {
        Console.WriteLine("WARNING: GenerateCoefficient using a predetermined nonce for testing purposes only. This should not be used in production.");
        var secret = BigMath.AddModQ(seed, (ulong)index);
        return new(offset, index, parameterHash, secret);
    }

    /// <summary>
    /// Generate a Coefficient for testing purposes only.
    /// In a production environment, nonce should be null and a random value will be generated.
    /// </summary>
    /// <param name="secret">A predetermined nonce parameter for testing purposes only.</param>
    /// <param name="index">The index of the coefficient</param>
    /// <param name="seed">A predetermined seed for testing purposes only.</param>
    public static Coefficient GenerateCoefficient(
        ulong offset, int index, ElementModQ parameterHash, ElementModQ nonce, ElementModQ seed)
    {
        Console.WriteLine("WARNING: GenerateCoefficient using a predetermined nonce for testing purposes only. This should not be used in production.");
        var secret = BigMath.AddModQ(nonce, (ulong)index);
        Console.WriteLine($"value: {secret}");
        return new(offset, index, parameterHash, secret, seed);
    }
}

public static class PolynomialExtensions
{
    /// <summary>
    /// Generate a Polynomial for testing purposes only.
    /// In a production environment, nonce should be null and a random value will be generated.
    /// </summary>
    /// <param name="secret">A predetermined nonce parameter for testing purposes only.</param>
    /// <param name="quorum">The number of coefficients in the polynomial</param>
    public static ElectionPolynomial GeneratePolynomial(ulong sequenceOrder, int quorum, ElementModQ secret)
    {
        Console.WriteLine("WARNING: GeneratePolynomial using a predetermined nonce for testing purposes only. This should not be used in production.");
        var coefficients = new List<Coefficient>();
        for (var i = 0; i < quorum; i++)
        {
            coefficients.Add(CoefficientExtensions.GenerateCoefficient(
                sequenceOrder,
                i,
                CiphertextElectionContext.ParameterBaseHash,
                secret));
        }
        return new(coefficients);
    }

    /// <summary>
    /// Generate a Polynomial for testing purposes only.
    /// In a production environment, nonce should be null and a random value will be generated.
    /// </summary>
    /// <param name="secret">A predetermined nonce parameter for testing purposes only.</param>
    /// <param name="quorum">The number of coefficients in the polynomial</param>
    /// <param name="seed">A predetermined seed for testing purposes only.</param>
    public static ElectionPolynomial GeneratePolynomial(ulong sequenceOrder, int quorum, ElementModQ secret, ElementModQ seed)
    {
        Console.WriteLine("WARNING: GeneratePolynomial using a predetermined nonce for testing purposes only. This should not be used in production.");
        var coefficients = new List<Coefficient>();
        for (var i = 0; i < quorum; i++)
        {
            coefficients.Add(CoefficientExtensions.GenerateCoefficient(
                sequenceOrder,
                i,
                CiphertextElectionContext.ParameterBaseHash,
                secret,
                seed));
        }
        return new(coefficients);
    }
}
