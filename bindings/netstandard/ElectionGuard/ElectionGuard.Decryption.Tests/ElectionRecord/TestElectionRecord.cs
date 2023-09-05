
using ElectionGuard.Decryption.Decryption;
using ElectionGuard.Decryption.ElectionRecord;
using ElectionGuard.Decryption.Tests.Decryption;
using ElectionGuard.ElectionSetup.Tests.Generators;
using ElectionGuard.Encryption.Utils.Generators;

namespace ElectionGuard.Decryption.Tests.ElectionRecord;

[TestFixture]
public class TestElectionRecord : DisposableBase
{
    private const int BALLOT_COUNT_CAST = 10;
    private const int BALLOT_COUNT_CHALLENGED = 2;
    private const int BALLOT_COUNT_SPOILED = 2;
    private const int NUMBER_OF_GUARDIANS = 5;
    private const int QUORUM = 3;

    private TestDecryptionData Data = default!;

    private DecryptionMediator Mediator = default!;

    private DecryptionResult Result = default!;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        RunSetup();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Data.Dispose();
    }

    [Test, Category("GenerateElectionRecord")]
    public async Task Test_Import_Export_Election_record()
    {
        // Arrange
        var electionRecord = new ElectionRecordData()
        {
            Constants = ElectionConstants.Current(),
            Guardians = Data.KeyCeremony.Guardians.Select(i => i.SharePublicKey()).ToList(),
            Manifest = Data.Election.Manifest,
            Context = Data.Election.Context,
            Devices = new List<EncryptionDevice>() { Data.Election.Device },
            EncryptedBallots = Data.CiphertextBallots,
            ChallengedBallots = Result.ChallengedBallots!,
            EncryptedTally = Data.CiphertextTally,
            Tally = Result.Tally!,
        };

        var path = "ElectionRecord";
        var subject = await ElectionRecordManager.ExportAsync(electionRecord, path);

        // Act
        var result = await ElectionRecordManager.ImportAsync(subject);

        // Assert
        Assert.That(result.Constants.P, Is.EqualTo(electionRecord.Constants.P));
    }

    private void RunSetup()
    {
        Data = TestDecryptionData.ConfigureTestCase(
            KeyCeremonyGenerator.GenerateKeyCeremonyData(
            NUMBER_OF_GUARDIANS,
            QUORUM, runKeyCeremony: true),
            ManifestGenerator.GetManifestFromFile(),
            BALLOT_COUNT_CAST,
            BALLOT_COUNT_CHALLENGED,
            BALLOT_COUNT_SPOILED);

        var guardians = Data.KeyCeremony.Guardians
                .GetRange(0, QUORUM)
                .ToList();

        Mediator = new DecryptionMediator(
            "fake-mediator",
            Data.CiphertextTally,
            Data.KeyCeremony.Guardians.Select(i => i.SharePublicKey()).ToList(),
            ballots: Data.CiphertextBallots.Where(i => i.IsSpoiled).ToList());

        Result = Mediator.RunDecryptionProcess(Data, guardians);
    }
}
