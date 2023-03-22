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

public class TestCiphertextTally
{
    private readonly ElementModQ nonce = Constants.ONE_MOD_Q;
    private readonly ElementModQ secret = Constants.TWO_MOD_Q;

    ElGamalKeyPair keyPair = default!;

    [SetUp]
    public void Setup()
    {
        keyPair = ElGamalKeyPair.FromSecret(secret);
    }

    [Test]
    public void Test_Accumulate_Cast_Ballots_Is_Valid()
    {
        // Arrange
        var data = ElectionGenerator.GenerateFakeElectionData();
        var count = 10UL;
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
        var result = mediator.CreateTally(
            "test", data.Context, data.InternalManifest);
        _ = result.Accumulate(encryptedBallots);

        var decryptedTally = result.Decrypt(data.KeyPair.SecretKey);

        // Assert
        Assert.That(plaintextTally, Is.EqualTo(decryptedTally));
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
    {

    }

}
