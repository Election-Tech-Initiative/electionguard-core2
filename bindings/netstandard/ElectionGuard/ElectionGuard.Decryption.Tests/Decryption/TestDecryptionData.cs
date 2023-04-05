
using ElectionGuard.Decryption.Tally;
using ElectionGuard.Decryption.Tests.Tally;
using ElectionGuard.ElectionSetup.Tests.Generators;
using ElectionGuard.Encryption.Utils.Generators;

namespace ElectionGuard.Decryption.Tests.Decryption;

// common elements used when running decryption tests
public class TestDecryptionData
{
    public TestElectionData Election { get; set; } = default!;
    public TestKeyCeremonyData KeyCeremony { get; set; } = default!;

    public List<PlaintextBallot> PlaintextBallots { get; set; } = default!;
    public List<CiphertextBallot> CiphertextBallots { get; set; } = default!;

    public PlaintextTally PlaintextTally { get; set; } = default!;
    public CiphertextTally CiphertextTally { get; set; } = default!;

    // configure a test case by running a key ceremony and generating ballots
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
