using ElectionGuard.Decryption.Tally;
using ElectionGuard.Encryption.Utils.Generators;

namespace ElectionGuard.Decryption.Tests.Tally;

public static class TestCiphertextTallyExtensions
{
    public static void AccumulateBallots(
        this PlaintextTally self, IList<PlaintextBallot> ballots)
    {
        var contestVotes = new Dictionary<string, int>();
        foreach (var contest in self.Contests)
        {
            contestVotes[contest.Key] = 0;
        }
        foreach (var ballot in ballots)
        {
            foreach (var contest in ballot.Contests)
            {
                contestVotes[contest.ObjectId] += 1;
                var contestTally = self.Contests[contest.ObjectId];
                foreach (var selection in contest.Selections)
                {
                    var selectionTally = contestTally.Selections[selection.ObjectId];
                    selectionTally.Tally += selection.Vote;
                }
            }
        }

        foreach (var item in contestVotes)
        {
            Console.WriteLine($"    Contest {item.Key} has {item.Value} ballots");
        }
    }
}

[TestFixture]
public class TestCiphertextTally : DisposableBase
{
    private TestElectionData Data = default!;
    private List<PlaintextBallot> PlaintextBallots = default!;
    private List<CiphertextBallot> CiphertextBallots = default!;

    // the count of unvalidated ballots to use in the test
    private const ulong BALLOT_COUNT_UNVALIDATED = 30UL;

    // the count of validated ballots to use in the test
    private const ulong BALLOT_COUNT_VALIDATED = 2UL;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        var random = new Random(1);
        Data = ElectionGenerator.GenerateFakeElectionData();
        PlaintextBallots = Enumerable.Range(0, (int)BALLOT_COUNT_UNVALIDATED)
            .Select(i =>
                BallotGenerator.GetFakeBallot(
                    Data.InternalManifest,
                    random, $"fake-ballot-{i}"))
            .ToList();

        // determioistically generate the seed and nonce
        var seed = random.NextElementModQ();
        var nonce = random.NextElementModQ();
        CiphertextBallots = PlaintextBallots.Select(
            ballot => Encrypt.Ballot(
                ballot,
                Data.InternalManifest,
                Data.Context, seed, nonce,
                shouldVerifyProofs: false))
            .ToList();
    }

    [SetUp]
    public void Setup()
    {

    }

    [TestCase(BALLOT_COUNT_VALIDATED, false)]
    [TestCase(BALLOT_COUNT_UNVALIDATED, true)]
    public void Test_Accumulate_Cast_Ballots_Is_Valid(
        ulong count, bool skipValidation)
    {
        Console.WriteLine($"--------------- {nameof(Test_Accumulate_Cast_Ballots_Is_Valid)} ------------------");
        Console.WriteLine($"    ballots: " + count);
        Console.WriteLine($"    skipValidation: " + skipValidation);

        // Arrange
        var plaintextBallots = Enumerable.Range(0, (int)count)
            .Select(i => PlaintextBallots[i].Copy()).ToList();
        var plaintextTally = new PlaintextTally("test-cast",
            Data.InternalManifest);
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

        var result = this.Benchmark(() =>
            ciphertextTally.Accumulate(ciphertextBallots, skipValidation),
            "Accumulate");

        Console.WriteLine($"    result: " + result.Result);
        Console.WriteLine($"    ciphertextTally: " + ciphertextTally);

        // Assert
        Assert.That(result.Result.Accepted, Has.Count.EqualTo(count));
        Assert.That(result.Result.Failed, Has.Count.EqualTo(0));

        var decryptedTally = this.Benchmark(() =>
            ciphertextTally.Decrypt(Data.KeyPair.SecretKey), "Decrypt");
        Assert.That(decryptedTally.Result, Is.EqualTo(plaintextTally));
    }

    [TestCase(BALLOT_COUNT_VALIDATED, false)]
    [TestCase(BALLOT_COUNT_UNVALIDATED, true)]
    public async Task Test_AccumulateAsync_Cast_Ballots_Is_Valid(
        ulong count, bool skipValidation)
    {
        Console.WriteLine($"--------------- {nameof(Test_AccumulateAsync_Cast_Ballots_Is_Valid)} ------------------");
        Console.WriteLine($"    ballots: " + count);
        Console.WriteLine($"    skipValidation: " + skipValidation);

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
            plaintextTally.TallyId,
            plaintextTally.Name,
            Data.Context,
            Data.InternalManifest);

        var result = await this.BenchmarkAsync(
            async () => await ciphertextTally.AccumulateAsync(ciphertextBallots, skipValidation), "AccumulateAsync");

        Console.WriteLine($"    result: " + result.Result);
        Console.WriteLine($"    ciphertextTally: " + ciphertextTally);

        // Assert
        Assert.That(result.Result.Accepted, Has.Count.EqualTo(count));
        Assert.That(result.Result.Failed, Has.Count.EqualTo(0));

        var decryptedTally = this.Benchmark(() =>
            ciphertextTally.Decrypt(Data.KeyPair.SecretKey), "Decrypt");
        Assert.That(decryptedTally.Result, Is.EqualTo(plaintextTally));
    }

    [TestCase(BALLOT_COUNT_VALIDATED, false)]
    [TestCase(BALLOT_COUNT_UNVALIDATED, true)]
    public void Test_Accumulate_Spoiled_Ballots_Is_Valid(
        ulong count, bool skipValidation)
    {
        Console.WriteLine($"--------------- {nameof(Test_Accumulate_Spoiled_Ballots_Is_Valid)} ------------------");
        Console.WriteLine($"    ballots: " + count);
        Console.WriteLine($"    skipValidation: " + skipValidation);

        // Arrange
        var plaintextBallots = Enumerable.Range(0, (int)count)
            .Select(i => PlaintextBallots[i].Copy()).ToList();
        var plaintextTally = new PlaintextTally("test-spoil",
            Data.InternalManifest);
        var ciphertextBallots = Enumerable.Range(0, (int)count)
            .Select(i =>
            {
                var encryptedBallot = CiphertextBallots[i].Copy();
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

        var result = this.Benchmark(() =>
            ciphertextTally.Accumulate(ciphertextBallots, skipValidation),
            "Accumulate");

        Console.WriteLine($"    result: " + result.Result);
        Console.WriteLine($"    ciphertextTally: " + ciphertextTally);

        // Assert
        Assert.That(result.Result.Accepted, Has.Count.EqualTo(count));
        Assert.That(result.Result.Failed, Has.Count.EqualTo(0));

        var decryptedTally = this.Benchmark(() =>
            ciphertextTally.Decrypt(Data.KeyPair.SecretKey), "Decrypt");
        Assert.That(decryptedTally.Result, Is.EqualTo(plaintextTally));
    }

    [TestCase(BALLOT_COUNT_UNVALIDATED, true)]
    public void Test_Accumulate_Invalid_Input_Fails(ulong count, bool skipValidation)
    {
        // Arrange
        var ciphertextBallots = Enumerable.Range(0, (int)count)
            .Select(i => CiphertextBallots[i].Copy()).ToList();
        var plaintextTally = new PlaintextTally("test-input", Data.InternalManifest);

        // Act
        var mediator = new TallyMediator();
        var ciphertextTally = mediator.CreateTally(
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

    [TestCase(BALLOT_COUNT_VALIDATED, false)]
    [TestCase(BALLOT_COUNT_UNVALIDATED, true)]
    public void Test_Accumulate_Cast_And_Spoiled_Ballots_Is_Valid(
        ulong count, bool skipValidation)
    {
        Console.WriteLine($"--------------- {nameof(Test_Accumulate_Cast_And_Spoiled_Ballots_Is_Valid)} ------------------");
        Console.WriteLine($"    ballots: " + count);
        Console.WriteLine($"    skipValidation: " + skipValidation);

        // Arrange
        var plaintextCastBallots = Enumerable.Range(0, (int)count / 2)
            .Select(i => PlaintextBallots[i].Copy()).ToList();
        var plaintextSpoiledBallots = Enumerable.Range((int)count / 2, (int)count / 2)
            .Select(i => PlaintextBallots[i].Copy()).ToList();
        var plaintextTally = new PlaintextTally("test-cast-and-spoil", Data.InternalManifest);
        plaintextTally.AccumulateBallots(plaintextCastBallots);

        Console.WriteLine($"    plaintextCastBallots: " + plaintextCastBallots.Count);
        Console.WriteLine($"    plaintextSpoiledBallots: " + plaintextSpoiledBallots.Count);

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

        var result = this.Benchmark(() =>
        {
            var castResult = ciphertextTally.Accumulate(ciphertextCastBallots);
            var spoiledResult = ciphertextTally.Accumulate(ciphertextSpoiledBallots);
            return castResult.Add(spoiledResult);
        }, "Accumulate");

        Console.WriteLine($"    result: " + result.Result);
        Console.WriteLine($"    ciphertextTally: " + ciphertextTally);

        // Assert
        Assert.That(result.Result.Accepted, Has.Count.EqualTo(count));
        Assert.That(result.Result.Failed, Has.Count.EqualTo(0));

        var decryptedTally = this.Benchmark(() =>
        ciphertextTally.Decrypt(Data.KeyPair.SecretKey), "Decrypt");
        Assert.That(decryptedTally.Result, Is.EqualTo(plaintextTally));
    }
}
