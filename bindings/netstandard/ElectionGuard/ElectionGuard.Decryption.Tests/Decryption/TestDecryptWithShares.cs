
using ElectionGuard.Encryption.Utils.Generators;
using ElectionGuard.ElectionSetup.Tests.Generators;
using ElectionGuard.Decryption.Tally;
using ElectionGuard.Decryption.Tests.Tally;
using ElectionGuard.Decryption.Decryption;
using ElectionGuard.Decryption.KeyCeremony;

namespace ElectionGuard.Decryption.Tests.Decryption;

public class TestDecryptionData
{
    public TestElectionData Election { get; set; } = default!;
    public TestKeyCeremonyData KeyCeremony { get; set; } = default!;

    public List<PlaintextBallot> PlaintextBallots { get; set; } = default!;
    public List<CiphertextBallot> CiphertextBallots { get; set; } = default!;

    public PlaintextTally PlaintextTally { get; set; } = default!;
    public CiphertextTally CiphertextTally { get; set; } = default!;

    public static TestDecryptionData ConfigureTestCase(
        TestKeyCeremonyData keyCeremony,
        Manifest manifest, int castBallotCount, int spoiledBallotCount)
    {
        var internalManifest = new InternalManifest(manifest);
        var context = new CiphertextElectionContext(
            (ulong)keyCeremony.NumberOfGuardians,
            (ulong)keyCeremony.Quorum,
            keyCeremony.JointKey,
            manifest.CryptoHash());

        var device = ElectionGenerator.GetFakeEncryptionDevice();
        var election = new TestElectionData
        {
            Manifest = manifest,
            InternalManifest = internalManifest,
            Context = context,
            Device = device
        };

        // generate the ballots
        var random = new Random(1);
        var plaintextBallots = BallotGenerator.GetFakeBallots(
            election.InternalManifest, random, castBallotCount + spoiledBallotCount);
        var ciphertextBallots = BallotGenerator.GetFakeCiphertextBallots(
            election.InternalManifest, election.Context, plaintextBallots, random);

        // cast and spoil the ballots
        // the spoiled ballots are the last ones in the list
        Enumerable.Range(0, castBallotCount).ToList().ForEach(i =>
        {
            ciphertextBallots[i]!.Cast();
        });
        Enumerable.Range(castBallotCount, spoiledBallotCount).ToList().ForEach(i =>
        {
            ciphertextBallots[i]!.Spoil();
        });

        // create a tally
        var plaintextCastBallots = Enumerable.Range(0, castBallotCount)
            .Select(i => plaintextBallots[i]!).ToList();
        var plaintextTally = new PlaintextTally(
            "test-decrypt-with-shares", election.InternalManifest);
        plaintextTally.AccumulateBallots(plaintextCastBallots);

        var mediator = new TallyMediator();
        var ciphertextTally = mediator.CreateTally(
            plaintextTally.TallyId,
            plaintextTally.Name,
            election.Context,
            election.InternalManifest);
        _ = ciphertextTally.Accumulate(ciphertextBallots);

        Console.WriteLine($"{nameof(ConfigureTestCase)} Setup Complete.");
        return new TestDecryptionData
        {
            Election = election,
            KeyCeremony = keyCeremony,
            PlaintextBallots = plaintextBallots,
            CiphertextBallots = ciphertextBallots,
            PlaintextTally = plaintextTally,
            CiphertextTally = ciphertextTally,
        };
    }
}

[TestFixture]
public class TestDecryptWithShares : DisposableBase
{
    // the count of unvalidated ballots to use in the test
    private const int BALLOT_COUNT_CAST = 30;

    // the count of validated ballots to use in the test
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
            BALLOT_COUNT_SPOILED);
    }

    [Test]
    public void Test_Decrypt_Selection_With_All_Guardians_Present_Simple()
    {
        // Arrange
        var castBallots = 3;
        var spoiledBallots = 2;
        var data = TestDecryptionData.ConfigureTestCase(
            KeyCeremonyGenerator.GenerateKeyCeremonyData(
            NUMBER_OF_GUARDIANS,
            QUORUM, runKeyCeremony: true),
            ManifestGenerator.GetFakeManifest(),
            castBallots,
            spoiledBallots);

        var mediator = new DecryptionMediator(
            "fake-mediator",
            data.CiphertextTally,
            data.KeyCeremony.Guardians.Select(i => i.ShareKey()).ToList());

        // Act
        foreach (var guardian in data.KeyCeremony.Guardians)
        {
            var share = guardian.ComputeDecryptionShare(data.CiphertextTally);
            mediator.SubmitShare(share!);
        }
        var plaintextTally = mediator.Decrypt(data.CiphertextTally.TallyId);

        // Assert
        Assert.That(plaintextTally, Is.EqualTo(data.PlaintextTally));
    }


    [Test]
    public void Test_Decrypt_Selection_With_All_Guardians_Present()
    {
        // Arrange
        var mediator = new DecryptionMediator(
            "fake-mediator",
            Data.CiphertextTally,
            Data.KeyCeremony.Guardians.Select(i => i.ShareKey()).ToList());

        // Act
        foreach (var guardian in Data.KeyCeremony.Guardians)
        {
            var share = guardian.ComputeDecryptionShare(Data.CiphertextTally);
            mediator.SubmitShare(share!);
        }
        var plaintextTally = mediator.Decrypt(Data.CiphertextTally.TallyId);

        // Assert
        Assert.That(plaintextTally, Is.EqualTo(Data.PlaintextTally));
    }

    [Test]
    public void Test_Decrypt_Selection_With_Quorum_Guardians_Present()
    {

    }

}
