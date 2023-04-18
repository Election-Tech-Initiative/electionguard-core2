using System;
using System.IO;

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

        // hold onto the nonces so we can decrypt the ballots
        var nonces = new Dictionary<string, ElementModQ>();

        // cast and spoil the ballots
        // the spoiled ballots are the last ones in the list
        Enumerable.Range(0, castBallotCount).ToList().ForEach(i =>
        {
            var nonce = ciphertextBallots[i]!.Nonce;
            nonces.Add(ciphertextBallots[i]!.ObjectId, new ElementModQ(nonce));
            ciphertextBallots[i]!.Cast();
        });
        Enumerable.Range(castBallotCount, spoiledBallotCount).ToList().ForEach(i =>
        {
            var nonce = ciphertextBallots[i]!.Nonce;
            nonces.Add(ciphertextBallots[i]!.ObjectId, new ElementModQ(nonce));
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

        var testData = JsonConvert.SerializeObject(data,
            SerializationSettings.NewtonsoftSettings());
        File.WriteAllText(Path.Combine(path, "test-data.json"), testData);

        var testResult = JsonConvert.SerializeObject(result,
            SerializationSettings.NewtonsoftSettings());
        File.WriteAllText(Path.Combine(path, "test-result.json"), testResult);
    }
}
