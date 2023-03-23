using ElectionGuard.Decryption.Tally;
using ElectionGuard.Encryption.Utils.Generators;

namespace ElectionGuard.Decryption.Tests;

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

public class TestCiphertextTally : DisposableBase
{
    private TestElectionData Data = default!;
    private List<PlaintextBallot> Ballots = default!;

    private EncryptionMediator Encryptor = default!;

    private static readonly int BALLOT_COUNT = 1000;

    [SetUp]
    public void Setup()
    {
        Data = ElectionGenerator.GenerateFakeElectionData();
        Ballots = Enumerable.Range(0, BALLOT_COUNT)
            .Select(i =>
                BallotGenerator.GetFakeBallot(Data.InternalManifest)).ToList();

        Encryptor = new EncryptionMediator(
            Data.InternalManifest, Data.Context, Data.Device);
    }

    [Test]
    public void Test_Accumulate_Cast_Ballots_Is_Valid()
    {
        // Arrange
        var data = ElectionGenerator.GenerateFakeElectionData();
        var count = 2UL;
        var ballots = Enumerable.Range(0, (int)count)
            .Select(i =>
                BallotGenerator.GetFakeBallot(data.InternalManifest)).ToList();
        var plaintextTally = new PlaintextTally("test", data.InternalManifest);
        plaintextTally.AccumulateBallots(ballots);

        var encryptor = new EncryptionMediator(
            data.InternalManifest, data.Context, data.Device);
        var encryptedBallots = ballots.Select(
            ballot =>
                {
                    var encryptedBallot = encryptor.Encrypt(ballot);
                    encryptedBallot!.Cast();
                    return encryptedBallot;
                }).ToList();


        // Act
        var mediator = new TallyMediator();
        var ciphertextTally = mediator.CreateTally(
            plaintextTally.TallyId,
            plaintextTally.Name,
            data.Context,
            data.InternalManifest);
        var result = ciphertextTally.Accumulate(encryptedBallots);

        var decryptedTally = ciphertextTally.Decrypt(data.KeyPair.SecretKey);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Accumulated, Has.Count.EqualTo(count));
            Assert.That(result.Failed, Has.Count.EqualTo(0));
            Assert.That(plaintextTally, Is.EqualTo(decryptedTally));
        });
    }

    [Test]
    public void Test_Accumulate_Spoiled_Ballots_Is_Valid()
    {

    }

    [Test]
    public void Test_Accumulate_Invalid_Input_Fails()
    {

    }

    [Test]
    public void Test_Accumulate_Async_Cast_And_Spoiled_Ballots_Is_Valid()
    private PlaintextBallot Copy(PlaintextBallot ballot)
    {
        var json = ballot.ToJson();
        return new PlaintextBallot(json);
    }

    protected override void DisposeUnmanaged()
    {
        Encryptor.Dispose();
        foreach (var ballot in Ballots)
        {
            ballot.Dispose();
        }
        Data.Dispose();
    }
}
