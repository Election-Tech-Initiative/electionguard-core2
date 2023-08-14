namespace ElectionGuard.ElectionSetup.Tests.KeyCeremony;

public class TestElectionPolynomial
{
    [Test]
    public void Test_ElectionPolynomial_Constructs_WithDegrees()
    {
        // Arrange
        var sequenceOrder = 2UL;
        var degrees = 2;
        var parameterHash = Constants.TWO_MOD_Q;

        // Act
        var result = new ElectionPolynomial(sequenceOrder, degrees, parameterHash);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Coefficients, Has.Count.EqualTo(degrees));
    }

    [Test]
    public void Test_ElectionPolynomial_Constructs_WithSecretKey()
    {
        // Arrange
        var sequenceOrder = 2UL;
        var degrees = 2;
        var parameterHash = Constants.TWO_MOD_Q;
        var secretKey = Constants.TWO_MOD_Q;

        // Act
        var result = new ElectionPolynomial(sequenceOrder, degrees, parameterHash, secretKey);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Coefficients, Has.Count.EqualTo(degrees));
        Assert.That(result.Coefficients[0].Value, Is.EqualTo(secretKey));
    }

    [Test]
    public void Test_ElectionPolynomial_Constructs_WithKeyPair()
    {
        // Arrange
        var sequenceOrder = 2UL;
        var degrees = 3;
        var parameterHash = Constants.TWO_MOD_Q;
        using var secretKey = BigMath.RandQ();
        using var keyPair = new ElGamalKeyPair(secretKey);

        // Act
        using var result = new ElectionPolynomial(sequenceOrder, degrees, parameterHash, keyPair);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Coefficients, Has.Count.EqualTo(degrees));
        Assert.That(result.Coefficients[0].Value, Is.EqualTo(keyPair.SecretKey));
    }

    // TODO: fix this test
    [Ignore("throws bad alloc in elgamal keypair")]
    [Test]
    public void Test_ElectionPolynomial_ComputeCoordinate()
    {
        // Arrange
        var offset = 0UL;
        var coordinate = 1UL;
        var degrees = 2;
        var nonce = Constants.ONE_MOD_Q;
        var seed = Constants.TWO_MOD_Q;
        var polynomial = PolynomialExtensions.GeneratePolynomial(offset, degrees, nonce, seed);

        // Act
        var result = polynomial.ComputeCoordinate(coordinate);

        // Assert
        Assert.That(result, Is.Not.Null);
        //Assert.That(result, Is.EqualTo(Constants.ONE_MOD_Q));
    }

    [Test]
    public void Test_ElectionPolynomial_VerifyCoordinate()
    {
        // Arrange
        var sequenceOrder = 2UL;
        var coordinate = 1UL;
        var degrees = 2UL;
        var parameterHash = Constants.TWO_MOD_Q;
        var polynomial = new ElectionPolynomial(sequenceOrder, (int)degrees, parameterHash);
        var expected = polynomial.ComputeCoordinate(coordinate);

        // Act
        var result = polynomial.VerifyCoordinate(coordinate, expected);

        // Assert
        Assert.That(result, Is.True);
    }
}
