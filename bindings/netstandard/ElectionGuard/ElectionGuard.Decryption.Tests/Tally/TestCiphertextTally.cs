using ElectionGuard.Decryption.Tally;
using ElectionGuard.Encryption.Utils.Generators;

namespace ElectionGuard.Decryption.Tests.Tally;

public static class TestCiphertextTallyExtensions
{
    public static void AccumulateBallots(
        this PlaintextTally self, IList<PlaintextBallot> ballots)
    {
        foreach (var ballot in ballots)
        {
            foreach (var contest in ballot.Contests)
            {
                var contestTally = self.Contests[contest.ObjectId];
                foreach (var selection in contest.Selections)
                {
                    var selectionTally = contestTally.Selections[selection.ObjectId];
                    selectionTally.Tally += selection.Vote;
                }
            }
        }
    }
}

[TestFixture]
public class TestCiphertextTally : DisposableBase
{
    private TestElectionData Data = default!;
    private List<PlaintextBallot> PlaintextBallots = default!;
    private List<CiphertextBallot> CiphertextBallots = default!;

    private EncryptionMediator Encryptor = default!;

    // the count of unvalidated ballots to use in the test
    private const ulong BALLOT_COUNT_UNVALIDATED = 30UL;

    // the count of validated ballots to use in the test
    private const ulong BALLOT_COUNT_VALIDATED = 2UL;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        var random = new Random(1);
        Data = ElectionGenerator.GenerateFakeElectionData();
        Encryptor = new EncryptionMediator(
            Data.InternalManifest, Data.Context, Data.Device);

        PlaintextBallots = Enumerable.Range(0, (int)BALLOT_COUNT_UNVALIDATED)
            .Select(i =>
                BallotGenerator.GetFakeBallot(
                    Data.InternalManifest,
                    random))
            .ToList();
        CiphertextBallots = PlaintextBallots.Select(
            ballot => Encryptor.Encrypt(ballot))
            .ToList();
    }

    [SetUp]
    public void Setup()
    {

    }

    [TestCase(BALLOT_COUNT_VALIDATED, false)]
    [TestCase(BALLOT_COUNT_UNVALIDATED, true)]
    public void Test_Accumulate_Cast_Ballots_Is_Valid(ulong count, bool skipValidation)
    {
        // Arrange
        var plaintextBallots = Enumerable.Range(0, (int)count)
            .Select(i => PlaintextBallots[i].Copy()).ToList();
        var plaintextTally = new PlaintextTally("test-cast", Data.InternalManifest);
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
            plaintextTally.TallyId,
            plaintextTally.Name,
            Data.Context,
            Data.InternalManifest);


        Console.WriteLine($"{nameof(Test_Accumulate_Cast_Ballots_Is_Valid)}: ballots: " + count);
        //var result = ciphertextTally.Accumulate(ciphertextBallots, skipValidation);
        var result = this.Benchmark(() =>
            ciphertextTally.Accumulate(ciphertextBallots, skipValidation), "Accumulate");

        // Assert
        Console.WriteLine($"{nameof(Test_Accumulate_Cast_Ballots_Is_Valid)}: result: {result.Result}");
        Assert.That(result.Result.Accumulated, Has.Count.EqualTo(count));
        Assert.That(result.Result.Failed, Has.Count.EqualTo(0));

        var decryptedTally = this.Benchmark(() =>
            ciphertextTally.Decrypt(Data.KeyPair.SecretKey), "Decrypt");
        Assert.That(plaintextTally, Is.EqualTo(decryptedTally.Result));
    }

    [Ignore("Not implemented")]
    [TestCase(BALLOT_COUNT_VALIDATED, false)]
    [TestCase(BALLOT_COUNT_UNVALIDATED, true)]
    public async Task Test_AccumulateAsync_Cast_Ballots_Is_Valid(ulong count, bool skipValidation)
    {
        // Arrange
        var plaintextBallots = Enumerable.Range(0, (int)count)
            .Select(i => PlaintextBallots[i].Copy()).ToList();
        var plaintextTally = new PlaintextTally("test-cast-async", Data.InternalManifest);
        plaintextTally.AccumulateBallots(plaintextBallots);
        var encryptedBallots = plaintextBallots.Select(
            ballot =>
                {
                    var encryptedBallot = Encryptor.Encrypt(ballot);
                    encryptedBallot!.Cast();
                    return encryptedBallot;
                }).ToList();


        // Act
        var mediator = new TallyMediator();
        var ciphertextTally = mediator.CreateTally(
            plaintextTally.TallyId,
            plaintextTally.Name,
            Data.Context,
            Data.InternalManifest);

        Console.WriteLine($"{nameof(Test_AccumulateAsync_Cast_Ballots_Is_Valid)}: ballots: " + count);
        var result = await this.BenchmarkAsync(
            async () => await ciphertextTally.AccumulateAsync(encryptedBallots, skipValidation), "AccumulateAsync");

        // Assert
        Console.WriteLine($"{nameof(Test_AccumulateAsync_Cast_Ballots_Is_Valid)}: result: {result.Result}");
        Assert.That(result.Result.Accumulated, Has.Count.EqualTo(count));
        Assert.That(result.Result.Failed, Has.Count.EqualTo(0));

        var decryptedTally = this.Benchmark(() => ciphertextTally.Decrypt(Data.KeyPair.SecretKey), "Decrypt");
        Assert.That(plaintextTally, Is.EqualTo(decryptedTally.Result));
    }

    [Ignore("Not implemented")]
    [TestCase(BALLOT_COUNT_VALIDATED, false)]
    [TestCase(BALLOT_COUNT_UNVALIDATED, true)]
    public void Test_Accumulate_Spoiled_Ballots_Is_Valid(ulong count, bool skipValidation)
    {
        // Arrange
        var plaintextBallots = Enumerable.Range(0, (int)count)
            .Select(i => PlaintextBallots[i].Copy()).ToList();
        var plaintextTally = new PlaintextTally("test-spoil", Data.InternalManifest);
        var encryptedBallots = plaintextBallots.Select(
            ballot =>
                {
                    var encryptedBallot = Encryptor.Encrypt(ballot);
                    encryptedBallot!.Spoil();
                    return encryptedBallot;
                }).ToList();

        // Act
        var mediator = new TallyMediator();
        var ciphertextTally = mediator.CreateTally(
            plaintextTally.TallyId,
            plaintextTally.Name,
            Data.Context,
            Data.InternalManifest);

        Console.WriteLine($"{nameof(Test_Accumulate_Spoiled_Ballots_Is_Valid)}: ballots: " + count);
        var result = this.Benchmark(() => ciphertextTally.Accumulate(encryptedBallots, skipValidation), "Accumulate");

        // Assert
        Console.WriteLine($"{nameof(Test_Accumulate_Spoiled_Ballots_Is_Valid)}: result: {result.Result}");
        Assert.That(result.Result.Accumulated, Has.Count.EqualTo(count));
        Assert.That(result.Result.Failed, Has.Count.EqualTo(0));

        var decryptedTally = this.Benchmark(() => ciphertextTally.Decrypt(Data.KeyPair.SecretKey), "Decrypt");
        Assert.That(plaintextTally, Is.EqualTo(decryptedTally.Result));
    }

    [Ignore("Not implemented")]
    [Test]
    public void Test_Accumulate_Invalid_Input_Fails()
    {

    }

    [Ignore("Not implemented")]
    [TestCase(BALLOT_COUNT_VALIDATED, false)]
    [TestCase(BALLOT_COUNT_UNVALIDATED, true)]
    public void Test_Accumulate_Async_Cast_And_Spoiled_Ballots_Is_Valid(ulong count, bool skipValidation)
    {
        // Arrange
        var plaintextCastBallots = Enumerable.Range(0, (int)count / 2)
            .Select(i => PlaintextBallots[i].Copy()).ToList();
        var plaintextSpoiledBallots = Enumerable.Range((int)count / 2, (int)count / 2)
            .Select(i => PlaintextBallots[i].Copy()).ToList();
        var plaintextTally = new PlaintextTally("test-spoil", Data.InternalManifest);
        plaintextTally.AccumulateBallots(plaintextCastBallots);

        var ciphertextCastBallots = plaintextCastBallots.Select(
            ballot =>
                {
                    var encryptedBallot = Encryptor.Encrypt(ballot);
                    encryptedBallot!.Cast();
                    return encryptedBallot;
                }).ToList();

        var ciphertextSpoiledBallots = plaintextSpoiledBallots.Select(
            ballot =>
                {
                    var encryptedBallot = Encryptor.Encrypt(ballot);
                    encryptedBallot!.Spoil();
                    return encryptedBallot;
                }).ToList();

        // Act
        var mediator = new TallyMediator();
        var ciphertextTally = mediator.CreateTally(
            plaintextTally.TallyId,
            plaintextTally.Name,
            Data.Context,
            Data.InternalManifest);

        Console.WriteLine($"{nameof(Test_Accumulate_Async_Cast_And_Spoiled_Ballots_Is_Valid)}: ballots: " + count);
        var result = this.Benchmark(() =>
        {
            var castResult = ciphertextTally.Accumulate(ciphertextCastBallots);
            var spoiledResult = ciphertextTally.Accumulate(ciphertextSpoiledBallots);
            return castResult.Add(spoiledResult);
        }, "Accumulate");

        // Assert
        Console.WriteLine($"{nameof(Test_Accumulate_Async_Cast_And_Spoiled_Ballots_Is_Valid)}: result: {result.Result}");
        Assert.That(result.Result.Accumulated, Has.Count.EqualTo(count));
        Assert.That(result.Result.Failed, Has.Count.EqualTo(0));

        var decryptedTally = this.Benchmark(() => ciphertextTally.Decrypt(Data.KeyPair.SecretKey), "Decrypt");
        Assert.That(plaintextTally, Is.EqualTo(decryptedTally.Result));
    }

    protected override void DisposeUnmanaged()
    {
        Encryptor.Dispose();
        foreach (var ballot in PlaintextBallots)
        {
            ballot.Dispose();
        }
        foreach (var ballot in CiphertextBallots)
        {
            ballot.Dispose();
        }
        Data.Dispose();
    }
}
