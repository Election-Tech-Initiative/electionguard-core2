
using ElectionGuard.Decryption.Tests.Tally;
using ElectionGuard.ElectionSetup.Tests.Generators;
using ElectionGuard.Encryption.Utils.Generators;

namespace ElectionGuard.Decryption.Tests.Decryption;

[TestFixture]
public class TestDecryptWithShares : DisposableBase
{
    private const int BALLOT_COUNT_CAST = 30;
    private const int BALLOT_COUNT_CHALLENGED = 2;
    private const int BALLOT_COUNT_SPOILED = 2;
    private const int NUMBER_OF_GUARDIANS = 5;
    private const int QUORUM = 3;

    private TestDecryptionData Data = default!;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        Data = TestDecryptionData.ConfigureTestCase(
            KeyCeremonyGenerator.GenerateKeyCeremonyData(
            NUMBER_OF_GUARDIANS,
            QUORUM, runKeyCeremony: true),
            ManifestGenerator.GetManifestFromFile(),
            BALLOT_COUNT_CAST,
            BALLOT_COUNT_CHALLENGED,
            BALLOT_COUNT_SPOILED);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Data.Dispose();
    }

    // Decrypt only the tally
    [Test]
    public void Test_Decrypt_Tally_With_All_Guardians_Present()
    {
        // Arrange
        var mediator = new DecryptionMediator(
            "fake-mediator",
            Data.CiphertextTally,
            Data.KeyCeremony.Guardians.Select(i => i.SharePublicKey()).ToList());

        // Act
        var guardians = Data.KeyCeremony.Guardians
                .ToList();
        var result = mediator.RunDecryptionProcess(Data, guardians, tallyOnly: true);

        // Assert
        var plaintextChallengedBallots = Data.CiphertextBallots
            .Where(i => Data.CiphertextTally.ChallengedBallotIds.Contains(i.ObjectId))
            .Select(i => i.ToTallyBallot(Data.PlaintextBallots.Single(j => j.ObjectId == i.ObjectId), Data.CiphertextTally)).ToList();
        Assert.That(result.Tally, Is.EqualTo(Data.PlaintextTally));
        Assert.That(result.ChallengedBallots!.Count, Is.EqualTo(0));
    }

    [Test]
    public void Test_Decrypt_With_All_Guardians_Present()
    {
        // Arrange
        var mediator = new DecryptionMediator(
            "fake-mediator",
            Data.CiphertextTally,
            Data.KeyCeremony.Guardians.Select(i => i.SharePublicKey()).ToList());

        // Act
        var guardians = Data.KeyCeremony.Guardians
                .ToList();
        var result = mediator.RunDecryptionProcess(Data, guardians);

        // Assert
        var plaintextChallengedBallots = Data.CiphertextBallots
            .Where(i => Data.CiphertextTally.ChallengedBallotIds.Contains(i.ObjectId))
            .Select(i => i.ToTallyBallot(
                Data.PlaintextBallots.Single(j => j.ObjectId == i.ObjectId), Data.CiphertextTally))
            .ToList();
        Assert.That(result.Tally, Is.EqualTo(Data.PlaintextTally));
        Assert.That(result.ChallengedBallots!.Count, Is.EqualTo(plaintextChallengedBallots.Count));
        Assert.That(result.ChallengedBallots, Is.EqualTo(plaintextChallengedBallots));
    }

    [Test]
    public void Test_Decrypt_With_Quorum_Guardians_Present()
    {
        // Arrange
        var mediator = new DecryptionMediator(
            "fake-mediator",
            Data.CiphertextTally,
            Data.KeyCeremony.Guardians.Select(i => i.SharePublicKey()).ToList());

        // Act
        var guardians = Data.KeyCeremony.Guardians
                .GetRange(0, QUORUM)
                .ToList();
        var result = mediator.RunDecryptionProcess(Data, guardians);

        // Assert
        var plaintextChallengedBallots = Data.CiphertextBallots
            .Where(plain => Data.CiphertextTally.ChallengedBallotIds.Contains(plain.ObjectId))
            .Select(cipher => cipher.ToTallyBallot(
                Data.PlaintextBallots.Single(plain => plain.ObjectId == cipher.ObjectId), Data.CiphertextTally))
                .ToList();
        Assert.That(result.Tally, Is.EqualTo(Data.PlaintextTally));
        Assert.That(result.ChallengedBallots!.Count, Is.EqualTo(plaintextChallengedBallots.Count));
        Assert.That(result.ChallengedBallots, Is.EqualTo(plaintextChallengedBallots));
    }

    [Test, Category("OutputTestData")]
    public void Test_Save_TestDataOutput()
    {
        // Arrange
        var mediator = new DecryptionMediator(
            "fake-mediator",
            Data.CiphertextTally,
            Data.KeyCeremony.Guardians.Select(i => i.SharePublicKey()).ToList());

        // Act
        var guardians = Data.KeyCeremony.Guardians
                .GetRange(0, QUORUM)
                .ToList();
        var result = mediator.RunDecryptionProcess(Data, guardians);

        // Assert
        TestDecryptionData.SaveToFile(Data, result);
    }
}
