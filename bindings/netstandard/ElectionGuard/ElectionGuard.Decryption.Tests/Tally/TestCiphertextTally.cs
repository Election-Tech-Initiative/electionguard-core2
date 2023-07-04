using ElectionGuard.Decryption.Decryption;
using ElectionGuard.Decryption.Tally;
using ElectionGuard.Encryption.Utils.Generators;

namespace ElectionGuard.Decryption.Tests.Tally;

[TestFixture]
public class TestCiphertextTally : DisposableBase
{
    private const ulong BALLOT_COUNT_LARGE = 30UL;
    private const ulong BALLOT_COUNT_SMALL = 2UL;

    private TestElectionData Data = default!;
    private List<PlaintextBallot> PlaintextBallots = default!;
    private List<CiphertextBallot> CiphertextBallots = default!;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        var random = new Random(1);
        Data = ElectionGenerator.GenerateFakeElectionData();
        PlaintextBallots = BallotGenerator.GetFakeBallots(
            Data.InternalManifest, random, (int)BALLOT_COUNT_LARGE);

        // deterministically generate the seed and nonce
        var seed = random.NextElementModQ();
        var nonce = random.NextElementModQ();
        CiphertextBallots = BallotGenerator.GetFakeCiphertextBallots(
            Data.InternalManifest, Data.Context, PlaintextBallots, seed, nonce);
    }

    [TestCase(BALLOT_COUNT_SMALL, false)]
    [TestCase(BALLOT_COUNT_LARGE, true)]
    public void Test_Accumulate_Cast_Ballots_Is_Valid(
        ulong count, bool skipValidation)
    {
        // Arrange
        var plaintextBallots = Enumerable.Range(0, (int)count)
            .Select(i => PlaintextBallots[i].Copy()).ToList();
        var plaintextTally = new PlaintextTally("test-cast",
            Data.InternalManifest);
        plaintextTally.AccumulateBallots(plaintextBallots);
        var ciphertextBallots = CiphertextBallots
            .Where(i => plaintextBallots.Any(j => j.ObjectId == i.ObjectId))
            .Select(ballot =>
            {
                var encryptedBallot = ballot.Copy();
                encryptedBallot!.Cast();
                return encryptedBallot;
            }).ToList();

        // Act
        var mediator = new TallyMediator();
        var ciphertextTally = mediator.CreateTally(
            // just using the plaintext tally id for the ciphertext tally id
            // so the equality comparison assertion succeeds
            plaintextTally.TallyId,
            plaintextTally.Name,
            Data.Context,
            Data.InternalManifest);

        var result = this.Benchmark(() =>
            ciphertextTally.Accumulate(ciphertextBallots, skipValidation),
            "Accumulate");

        // Assert
        Assert.That(result.Result.Accepted, Has.Count.EqualTo(count));
        Assert.That(result.Result.Failed, Has.Count.EqualTo(0));

        var decryptedTally = this.Benchmark(() =>
            ciphertextTally.Decrypt(
                Data.KeyPair.SecretKey, Data.Context.ElGamalPublicKey), "Decrypt");
        Assert.That(decryptedTally.Result, Is.EqualTo(plaintextTally));
    }

    [TestCase(BALLOT_COUNT_SMALL, false)]
    [TestCase(BALLOT_COUNT_LARGE, true)]
    public async Task Test_AccumulateAsync_Cast_Ballots_Is_Valid(
        ulong count, bool skipValidation)
    {
        // Arrange
        var plaintextBallots = Enumerable.Range(0, (int)count)
            .Select(i => PlaintextBallots[i].Copy()).ToList();
        var plaintextTally = new PlaintextTally("test-cast-async", Data.InternalManifest);
        plaintextTally.AccumulateBallots(plaintextBallots);
        var ciphertextBallots = Enumerable.Range(0, (int)count)
            .Select(i =>
            {
                var encryptedBallot = CiphertextBallots[i].Copy();
                encryptedBallot!.Cast();
                return encryptedBallot;
            }).ToList();

        // Act
        var mediator = new TallyMediator();
        var ciphertextTally = mediator.CreateTally(
            // just using the plaintext tally id for the ciphertext tally id
            // so the equality comparison assertion succeeds
            plaintextTally.TallyId,
            plaintextTally.Name,
            Data.Context,
            Data.InternalManifest);

        var result = await this.BenchmarkAsync(
            async () => await ciphertextTally.AccumulateAsync(
                ciphertextBallots, skipValidation),
            "AccumulateAsync");

        // Assert
        Assert.That(result.Result.Accepted, Has.Count.EqualTo(count));
        Assert.That(result.Result.Failed, Has.Count.EqualTo(0));

        var decryptedTally = this.Benchmark(() =>
            ciphertextTally.Decrypt(
                Data.KeyPair.SecretKey,
                Data.Context.ElGamalPublicKey), "Decrypt");
        Assert.That(decryptedTally.Result, Is.EqualTo(plaintextTally));
    }

    [TestCase(BALLOT_COUNT_SMALL, false)]
    [TestCase(BALLOT_COUNT_LARGE, true)]
    public void Test_Accumulate_Challenged_Ballots_Is_Valid(
        ulong count, bool skipValidation)
    {
        // Arrange
        var plaintextTally = new PlaintextTally("test-challenge",
            Data.InternalManifest);
        var ciphertextBallots = Enumerable.Range(0, (int)count)
            .Select(i =>
            {
                var encryptedBallot = CiphertextBallots[i].Copy();
                encryptedBallot!.Challenge();
                return encryptedBallot;
            }).ToList();

        // Act
        var mediator = new TallyMediator();
        var ciphertextTally = mediator.CreateTally(
            // just using the plaintext tally id for the ciphertext tally id
            // so the equality comparison assertion succeeds
            plaintextTally.TallyId,
            plaintextTally.Name,
            Data.Context,
            Data.InternalManifest);

        var result = this.Benchmark(() =>
            ciphertextTally.Accumulate(ciphertextBallots, skipValidation),
            "Accumulate");

        // Assert
        Assert.That(result.Result.Accepted, Has.Count.EqualTo(count));
        Assert.That(result.Result.Failed, Has.Count.EqualTo(0));

        var decryptedTally = this.Benchmark(() =>
            ciphertextTally.Decrypt(
                Data.KeyPair.SecretKey,
                Data.Context.ElGamalPublicKey), "Decrypt");
        Assert.That(decryptedTally.Result, Is.EqualTo(plaintextTally));
    }

    [TestCase(BALLOT_COUNT_LARGE, true)]
    public void Test_Accumulate_Invalid_Input_Fails(ulong count, bool skipValidation)
    {
        // Arrange
        var ciphertextBallots = Enumerable.Range(0, (int)count)
            .Select(i => CiphertextBallots[i].Copy()).ToList();
        var plaintextTally = new PlaintextTally("test-input", Data.InternalManifest);

        // Act
        var mediator = new TallyMediator();
        var ciphertextTally = mediator.CreateTally(
            // just using the plaintext tally id for the ciphertext tally id
            // so the equality comparison assertion succeeds
            plaintextTally.TallyId,
            plaintextTally.Name,
            Data.Context,
            Data.InternalManifest);

        // Assert
        _ = Assert.Throws(typeof(ArgumentException), () =>
        {
            _ = ciphertextTally.Accumulate(ciphertextBallots, skipValidation);
        });
    }

    [TestCase(BALLOT_COUNT_SMALL, false)]
    [TestCase(BALLOT_COUNT_LARGE, true)]
    public void Test_Accumulate_Cast_And_Challenged_Ballots_Is_Valid(
        ulong count, bool skipValidation)
    {
        // Arrange
        var plaintextCastBallots = Enumerable.Range(0, (int)count / 2)
            .Select(i => PlaintextBallots[i].Copy()).ToList();
        var plaintextSpoiledBallots = Enumerable.Range((int)count / 2, (int)count / 2)
            .Select(i => PlaintextBallots[i].Copy()).ToList();
        var plaintextTally = new PlaintextTally(
            "test-cast-and-spoil", Data.InternalManifest);
        plaintextTally.AccumulateBallots(plaintextCastBallots);

        var ciphertextCastBallots = Enumerable.Range(0, (int)count / 2)
            .Select(i =>
            {
                var encryptedBallot = CiphertextBallots[i].Copy();
                encryptedBallot!.Cast();
                return encryptedBallot;
            }).ToList();

        var ciphertextSpoiledBallots = Enumerable.Range((int)count / 2, (int)count / 2)
            .Select(i =>
            {
                var encryptedBallot = CiphertextBallots[i].Copy();
                encryptedBallot!.Challenge();
                return encryptedBallot;
            }).ToList();

        // Act
        var mediator = new TallyMediator();
        var ciphertextTally = mediator.CreateTally(
            // just using the plaintext tally id for the ciphertext tally id
            // so the equality comparison assertion succeeds
            plaintextTally.TallyId,
            plaintextTally.Name,
            Data.Context,
            Data.InternalManifest);

        var result = this.Benchmark(() =>
        {
            var castResult = ciphertextTally.Accumulate(
                ciphertextCastBallots, skipValidation);
            var spoiledResult = ciphertextTally.Accumulate(
                ciphertextSpoiledBallots, skipValidation);
            return castResult.Add(spoiledResult);
        }, "Accumulate");

        // Assert
        Assert.That(result.Result.Accepted, Has.Count.EqualTo(count));
        Assert.That(result.Result.Failed, Has.Count.EqualTo(0));

        var decryptedTally = this.Benchmark(() =>
        ciphertextTally.Decrypt(
            Data.KeyPair.SecretKey,
            Data.Context.ElGamalPublicKey), "Decrypt");
        Assert.That(decryptedTally.Result, Is.EqualTo(plaintextTally));
    }

    [TestCase(BALLOT_COUNT_LARGE, false)]
    public void Test_Accumulate_Can_Combine_Unique_Tallies(
        ulong count, bool skipValidation)
    {
        // Arrange
        var firstCount = (int)count / 2;
        var secondCount = (int)count / 2;
        var plaintextCastBallots_1 = Enumerable.Range(0, firstCount - 1)
            .Select(i => PlaintextBallots[i].Copy()).ToList();
        var plaintextCastBallots_2 = Enumerable.Range(firstCount, secondCount)
            .Select(i => PlaintextBallots[i].Copy()).ToList();
        var plaintextTally = new PlaintextTally(
            "test-combine-tallies", Data.InternalManifest);

        // Add all of the ballots to the plaintext tallies
        plaintextTally.AccumulateBallots(plaintextCastBallots_1);
        plaintextTally.AccumulateBallots(plaintextCastBallots_2);

        // Create 2 separate tallies
        var ciphertextCastBallots_1 = Enumerable.Range(0, firstCount - 1)
            .Select(i =>
            {
                var encryptedBallot = CiphertextBallots[i].Copy();
                encryptedBallot!.Cast();
                return encryptedBallot;
            }).ToList();

        var ciphertextCastBallots_2 = Enumerable.Range(firstCount, secondCount)
            .Select(i =>
            {
                var encryptedBallot = CiphertextBallots[i].Copy();
                encryptedBallot!.Cast();
                return encryptedBallot;
            }).ToList();

        // Act
        var mediator = new TallyMediator();
        var ciphertextTally_1 = mediator.CreateTally(
            // just using the plaintext tally id for the ciphertext tally id
            // so the equality comparison assertion succeeds
            plaintextTally.TallyId,
            plaintextTally.Name,
            Data.Context,
            Data.InternalManifest);
        var ciphertextTally_2 = mediator.CreateTally(
            $"{plaintextTally.TallyId}-2",
            Data.Context,
            Data.InternalManifest);

        // Accumulate the 2 separate tallies
        _ = ciphertextTally_1.Accumulate(
               ciphertextCastBallots_1, skipValidation);
        _ = ciphertextTally_2.Accumulate(
           ciphertextCastBallots_2, skipValidation);

        // combine the tallies
        var result = this.Benchmark(() =>
        {
            var combineResult = ciphertextTally_1.Accumulate(
                ciphertextTally_2, skipValidation);
            return combineResult;

        }, "Combine");

        Console.WriteLine(result.Result);

        // Assert
        Assert.That(result.Result.Accepted, Has.Count.EqualTo(secondCount));
        Assert.That(result.Result.Failed, Has.Count.EqualTo(0));

        var decryptedTally = this.Benchmark(() =>
        ciphertextTally_1.Decrypt(
            Data.KeyPair.SecretKey,
            Data.Context.ElGamalPublicKey), "Decrypt");
        Assert.That(decryptedTally.Result, Is.EqualTo(plaintextTally));
    }
}
