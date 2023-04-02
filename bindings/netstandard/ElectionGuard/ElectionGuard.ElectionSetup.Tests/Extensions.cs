namespace ElectionGuard.ElectionSetup.Tests;

public static class CoefficientExtensions
{
    /// <summary>
    /// Generate a Coefficient for testing purposes only.
    /// In a production environment, nonce should be null and a random value will be generated.
    /// </summary>
    public static Coefficient GenerateCoefficient(ElementModQ nonce, int index)
    {
        Console.WriteLine("WARNING: GenerateCoefficient using a predetermined nonce for testing purposes only. This should not be used in production.");
        var value = BigMath.AddModQ(nonce, (ulong)index);
        return new(value);
    }

    /// <summary>
    /// Generate a Coefficient for testing purposes only.
    /// In a production environment, nonce should be null and a random value will be generated.
    /// </summary>
    /// <param name="nonce">A predetermined nonce parameter for testing purposes only.</param>
    /// <param name="index">The index of the coefficient</param>
    /// <param name="seed">A predetermined seed for testing purposes only.</param>
    public static Coefficient GenerateCoefficient(ElementModQ nonce, ElementModQ seed, int index)
    {
        Console.WriteLine("WARNING: GenerateCoefficient using a predetermined nonce for testing purposes only. This should not be used in production.");
        var value = BigMath.AddModQ(nonce, (ulong)index);
        Console.WriteLine($"value: {value}");
        return new(value, seed);
    }
}

public static class PolynomialExtensions
{
    /// <summary>
    /// Generate a Polynomial for testing purposes only.
    /// In a production environment, nonce should be null and a random value will be generated.
    /// </summary>
    /// <param name="nonce">A predetermined nonce parameter for testing purposes only.</param>
    /// <param name="quorum">The number of coefficients in the polynomial</param>
    public static ElectionPolynomial GeneratePolynomial(ElementModQ nonce, int quorum)
    {
        Console.WriteLine("WARNING: GeneratePolynomial using a predetermined nonce for testing purposes only. This should not be used in production.");
        var coefficients = new List<Coefficient>();
        for (var i = 0; i < quorum; i++)
        {
            coefficients.Add(CoefficientExtensions.GenerateCoefficient(nonce, i));
        }
        return new(coefficients);
    }

    /// <summary>
    /// Generate a Polynomial for testing purposes only.
    /// In a production environment, nonce should be null and a random value will be generated.
    /// </summary>
    /// <param name="nonce">A predetermined nonce parameter for testing purposes only.</param>
    /// <param name="quorum">The number of coefficients in the polynomial</param>
    /// <param name="seed">A predetermined seed for testing purposes only.</param>
    public static ElectionPolynomial GeneratePolynomial(ElementModQ nonce, ElementModQ seed, int quorum)
    {
        Console.WriteLine("WARNING: GeneratePolynomial using a predetermined nonce for testing purposes only. This should not be used in production.");
        var coefficients = new List<Coefficient>();
        for (var i = 0; i < quorum; i++)
        {
            coefficients.Add(CoefficientExtensions.GenerateCoefficient(nonce, seed, i));
        }
        return new(coefficients);
    }
}
