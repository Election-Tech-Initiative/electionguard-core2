using ElectionGuard.Decryption.Decryption;
using ElectionGuard.Decryption.Tally;
using ElectionGuard.Decryption.Tests.Tally;
using ElectionGuard.ElectionSetup.Tests.Generators;
using ElectionGuard.Encryption.Utils.Generators;
using ElectionGuard.Encryption.Utils.Converters;
using Newtonsoft.Json;
using ElectionGuard.UI.Lib.Extensions;

namespace ElectionGuard.Decryption.Tests.Decryption;

// common elements used when running decryption tests
public class TestDecryptionData : DisposableBase
{
    public TestElectionData Election { get; set; } = default!;
    public TestKeyCeremonyData KeyCeremony { get; set; } = default!;

    public List<PlaintextBallot> PlaintextBallots { get; set; } = default!;
    public List<CiphertextBallot> CiphertextBallots { get; set; } = default!;

    public Dictionary<string, ElementModQ> Nonces { get; set; } = default!;

    public PlaintextTally PlaintextTally { get; set; } = default!;
    public CiphertextTally CiphertextTally { get; set; } = default!;

    // configure a test case by running a key ceremony and generating ballots
    public static TestDecryptionData ConfigureTestCase(
        TestKeyCeremonyData keyCeremony,
        Manifest manifest,
        int castBallotCount,
        int challengedBallotCount,
        int spoiledBallotCount, Random? random = null)
    {
        random ??= new Random(1);
        var ballotCount = castBallotCount + challengedBallotCount + spoiledBallotCount;

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
        var plaintextBallots = BallotGenerator.GetFakeBallots(
            election.InternalManifest, random, ballotCount);
        var ciphertextBallots = BallotGenerator.GetFakeCiphertextBallots(
            election.InternalManifest, election.Context, plaintextBallots, random);

        // cast, challenge and spoil the ballots
        IList<BallotBoxState> ballotBoxStates = new List<BallotBoxState>();
        Enumerable.Range(0, castBallotCount).ToList().ForEach(i =>
        {
            ballotBoxStates.Add(BallotBoxState.Cast);
        });
        Enumerable.Range(0, challengedBallotCount).ToList().ForEach(i =>
        {
            ballotBoxStates.Add(BallotBoxState.Challenged);
        });
        Enumerable.Range(0, spoiledBallotCount).ToList().ForEach(i =>
        {
            ballotBoxStates.Add(BallotBoxState.Spoiled);
        });
        ballotBoxStates = ballotBoxStates.Shuffle(random);

        // hold onto the nonces so we can decrypt the ballots
        var nonces = new Dictionary<string, ElementModQ>();

        var plaintextCastBallots = new List<PlaintextBallot>();

        // update the ballot box states
        for (var index = 0; index < ballotCount; index++)
        {
            var ballot = ciphertextBallots[index];
            nonces.Add(ballot.ObjectId, new ElementModQ(ballot.Nonce));
            var state = ballotBoxStates[index];
            switch (state)
            {
                case BallotBoxState.Cast:
                    ballot.Cast();
                    plaintextCastBallots.Add(plaintextBallots[index]);
                    break;
                case BallotBoxState.Challenged:
                    ballot.Challenge();
                    break;
                case BallotBoxState.Spoiled:
                    ballot.Spoil();
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"invalid state for ballot {ballot.ObjectId}");
            }
        }

        Console.WriteLine($"{nameof(ConfigureTestCase)} Ballots Generated.");
        Console.WriteLine($" - Cast: {castBallotCount}");
        Console.WriteLine($" - Challenged: {challengedBallotCount}");
        Console.WriteLine($" - Spoiled: {spoiledBallotCount}");

        // create a tally
        var plaintextTally = new PlaintextTally(
            "test-decrypt-with-shares", election.InternalManifest);
        plaintextTally.AccumulateBallots(plaintextCastBallots);

        var mediator = new TallyMediator();
        var ciphertextTally = mediator.CreateTally(
            plaintextTally.TallyId,
            plaintextTally.Name,
            election.Context,
            election.InternalManifest);
        var addResult = ciphertextTally.Accumulate(ciphertextBallots);

        Console.WriteLine($"{ciphertextTally}");
        Console.WriteLine($"{addResult}");
        Console.WriteLine($"{nameof(ConfigureTestCase)} Setup Complete.");

        return new TestDecryptionData
        {
            Election = election,
            KeyCeremony = keyCeremony,
            PlaintextBallots = plaintextBallots,
            CiphertextBallots = ciphertextBallots,
            Nonces = nonces,
            PlaintextTally = plaintextTally,
            CiphertextTally = ciphertextTally,
        };
    }

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();

        Election.Dispose();
        KeyCeremony.Dispose();
        PlaintextBallots.Dispose();
        CiphertextBallots.Dispose();
        foreach (var nonce in Nonces.Values)
        {
            nonce.Dispose();
        }
        PlaintextTally.Dispose();
        CiphertextTally.Dispose();
    }

    public static void SaveToFile(TestDecryptionData data, DecryptionResult result)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "data");
        var directoryPath = Path.GetDirectoryName(path);
        _ = Directory.CreateDirectory(directoryPath!);

        // write data out to individual files (used by the cli and some tests)
        var context = JsonConvert.SerializeObject(data.Election.Context,
            SerializationSettings.NewtonsoftSettings());
        File.WriteAllText(Path.Combine(path, "context.json"), context);

        var manifest = JsonConvert.SerializeObject(data.Election.Manifest,
            SerializationSettings.NewtonsoftSettings());
        File.WriteAllText(Path.Combine(path, "manifest.json"), manifest);

        var device = JsonConvert.SerializeObject(data.Election.Device,
            SerializationSettings.NewtonsoftSettings());
        File.WriteAllText(Path.Combine(path, "device.json"), device);

        _ = Directory.CreateDirectory(Path.Combine(path, "plaintext"));
        foreach (var ballot in data.PlaintextBallots)
        {
            var plaintext = JsonConvert.SerializeObject(ballot,
                SerializationSettings.NewtonsoftSettings());
            File.WriteAllText(Path.Combine(path, "plaintext", $"{ballot.ObjectId}.json"), plaintext);
        }

        // write all of it in two files (used by typescript tests)
        var testData = JsonConvert.SerializeObject(data,
            SerializationSettings.NewtonsoftSettings());
        File.WriteAllText(Path.Combine(path, "test-data.json"), testData);

        var testResult = JsonConvert.SerializeObject(result,
            SerializationSettings.NewtonsoftSettings());
        File.WriteAllText(Path.Combine(path, "test-result.json"), testResult);
    }
}
