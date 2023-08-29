using System.IO.Compression;
using ElectionGuard.Converters;
using ElectionGuard.Decryption.Tally;
using ElectionGuard.Guardians;

namespace ElectionGuard.Decryption.ElectionRecord;

public static class ElectionRecordManager
{
    public static readonly string GUARDIAN_FOLDER = "guardians";
    public static readonly string DEVICE_FOLDER = "encryption_devices";
    public static readonly string BALLOT_FOLDER = "submitted_ballots";
    public static readonly string CHALLENGED_FOLDER = "spoiled_ballots";

    public static readonly string GUARDIAN_PREFIX = "guardian_";
    public static readonly string DEVICE_PREFIX = "device_";
    public static readonly string FOLDER_PREFIX = "eg_";

    public static readonly string MANIFEST_FILENAME = "manifest.json";
    public static readonly string CONTEXT_FILENAME = "context.json";
    public static readonly string CONSTANTS_FILENAME = "constants.json";
    public static readonly string COEFFICIENTS_FILENAME = "coefficients.json";
    public static readonly string TALLY_FILENAME = "tally.json";
    public static readonly string ENCRYPTED_TALLY_FILENAME = "encrypted_tally.json";

    public static async Task<ElectionRecordData> ImportAsync(string zipFile)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(zipFile));

        // create temp folder to unzip to
        var tempFolder = Directory.CreateTempSubdirectory(FOLDER_PREFIX);

        // unzip the file
        ZipFile.ExtractToDirectory(zipFile, tempFolder.FullName);

        // import the parts
        var record = await ImportParts(tempFolder.FullName);

        // delete the temp folder
        Directory.Delete(tempFolder.FullName, true);

        return record;
    }

    public static async Task<string> ExportAsync(ElectionRecordData record, string outputFolder)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(outputFolder));

        var outputFolderInfo = Directory.CreateDirectory(outputFolder);

        // generate temp path to export all of the files to
        var tempFolder = Directory.CreateTempSubdirectory(FOLDER_PREFIX);

        // export the individual parts of the record
        await ExportParts(record, tempFolder.FullName);

        // create the zip from the folder
        var zipFile = Path.Combine(
            outputFolderInfo.FullName,
            record.Tally.Name!.Replace(" ", "_") + ".zip");
        if (File.Exists(zipFile))
        {
            File.Delete(zipFile);
        }

        ZipFile.CreateFromDirectory(tempFolder.FullName, zipFile);

        // delete the temp folder
        Directory.Delete(tempFolder.FullName, true);

        return zipFile;
    }

    private static async Task<ElectionRecordData> ImportParts(string tempFolder)
    {
        // import the guardians
        var guardians = await ImportGuardiansAsync(Path.Combine(tempFolder, GUARDIAN_FOLDER));

        // import the top level
        var constants = await ImportConstantsAsync(Path.Combine(tempFolder, CONSTANTS_FILENAME));
        var manifest = await ImportManifestAsync(Path.Combine(tempFolder, MANIFEST_FILENAME));
        var context = await ImportContextAsync(Path.Combine(tempFolder, CONTEXT_FILENAME));

        // import the devices
        var devices = await ImportDevicesAsync(Path.Combine(tempFolder, DEVICE_FOLDER));

        // import the ballots
        var ballots = await ImportBallotsAsync(Path.Combine(tempFolder, BALLOT_FOLDER));

        // import the challenged ballots v2 / spoiled ballots v1
        var challengedBallots = await ImportChallengedBallotsAsync(Path.Combine(tempFolder, CHALLENGED_FOLDER));

        var encryptedTally = await ImportEncryptedTallyAsync(Path.Combine(tempFolder, ENCRYPTED_TALLY_FILENAME));

        var tally = await ImportTallyAsync(Path.Combine(tempFolder, TALLY_FILENAME));

        return new ElectionRecordData
        {
            Guardians = guardians,
            Constants = constants,
            Manifest = manifest,
            Context = context,
            Devices = devices,
            EncryptedBallots = ballots,
            ChallengedBallots = challengedBallots,
            EncryptedTally = encryptedTally,
            Tally = tally
        };
    }

    private static async Task<List<ElectionPublicKey>> ImportGuardiansAsync(string guardianFolder)
    {
        var guardians = new List<ElectionPublicKey>();

        foreach (var file in Directory.EnumerateFiles(guardianFolder))
        {
            var guardian = await ImportAsync<ElectionPublicKey>(file);
            guardians.Add(guardian);
        }

        return guardians;
    }

    private static async Task<ElectionConstants> ImportConstantsAsync(string jsonFile)
    {
        return await ImportAsync<ElectionConstants>(jsonFile);
    }

    private static async Task<Manifest> ImportManifestAsync(string jsonFile)
    {
        var json = await File.ReadAllTextAsync(jsonFile);
        return new Manifest(json);
    }

    private static async Task<CiphertextElectionContext> ImportContextAsync(string jsonFile)
    {
        var json = await File.ReadAllTextAsync(jsonFile);
        return new CiphertextElectionContext(json);
    }

    private static async Task<List<EncryptionDevice>> ImportDevicesAsync(string deviceFolder)
    {
        var devices = new List<EncryptionDevice>();

        foreach (var file in Directory.EnumerateFiles(deviceFolder))
        {
            var json = await File.ReadAllTextAsync(file);
            var device = new EncryptionDevice(json);
            devices.Add(device);
        }

        return devices;
    }

    private static async Task<List<CiphertextBallot>> ImportBallotsAsync(string ballotFolder)
    {
        var ballots = new List<CiphertextBallot>();

        foreach (var file in Directory.EnumerateFiles(ballotFolder))
        {
            var json = await File.ReadAllTextAsync(file);
            var ballot = new CiphertextBallot(json);
            ballots.Add(ballot);
        }

        return ballots;
    }

    private static async Task<List<PlaintextTallyBallot>> ImportChallengedBallotsAsync(string challengedFolder)
    {
        var ballots = new List<PlaintextTallyBallot>();

        foreach (var file in Directory.EnumerateFiles(challengedFolder))
        {

            var ballot = await ImportAsync<PlaintextTallyBallot>(file);
            ballots.Add(ballot);
        }

        return ballots;
    }

    private static async Task<PlaintextTally> ImportTallyAsync(string jsonFile)
    {
        return await ImportAsync<PlaintextTally>(jsonFile);
    }

    private static async Task<CiphertextTallyRecord> ImportEncryptedTallyAsync(string jsonFile)
    {
        return await ImportAsync<CiphertextTallyRecord>(jsonFile);
    }

    private static async Task<T> ImportAsync<T>(string jsonFile)
    {
        var json = await File.ReadAllTextAsync(jsonFile);
        return JsonConvert.DeserializeObject<T>(json, SerializationSettings.NewtonsoftSettings())!;
    }

    private static async Task ExportParts(ElectionRecordData record, string tempFolder)
    {
        // export the guardians
        await ExportGuardiansAsync(Path.Combine(tempFolder, GUARDIAN_FOLDER), record.Guardians!);

        // export the top level
        await ExportSummaryAsync(tempFolder, record.Constants, record.Manifest, record.Context);

        // export the devices
        await ExportDevices(Path.Combine(tempFolder, DEVICE_FOLDER), record.Devices!);

        // export the ballots
        await ExportBallotsAsync(Path.Combine(tempFolder, BALLOT_FOLDER), record.EncryptedBallots);

        // export the challenged ballots v2 / spoiled ballots v1
        await ExportChallengedBallotsAsync(Path.Combine(tempFolder, CHALLENGED_FOLDER), record.ChallengedBallots);

        await ExportEncryptedTallyAsync(Path.Combine(tempFolder, ENCRYPTED_TALLY_FILENAME), record.EncryptedTally);

        await ExportTallyAsync(Path.Combine(tempFolder, TALLY_FILENAME), record.Tally);
    }

    private static async Task ExportSummaryAsync(string tempFolder, ElectionConstants constants, Manifest manifest, CiphertextElectionContext context)
    {
        await File.WriteAllTextAsync(Path.Combine(tempFolder, CONSTANTS_FILENAME), JsonConvert.SerializeObject(
            constants,
            SerializationSettings.NewtonsoftSettings()));

        await File.WriteAllTextAsync(Path.Combine(tempFolder, MANIFEST_FILENAME), JsonConvert.SerializeObject(
            manifest,
            SerializationSettings.NewtonsoftSettings()));

        await File.WriteAllTextAsync(Path.Combine(tempFolder, CONTEXT_FILENAME), JsonConvert.SerializeObject(
            context,
            SerializationSettings.NewtonsoftSettings()));
    }

    private static async Task ExportGuardiansAsync(string guardianFolder, IEnumerable<ElectionPublicKey> guardians)
    {
        _ = Directory.CreateDirectory(guardianFolder);

        foreach (var guardian in guardians)
        {
            var filename = Path.Combine(
                guardianFolder, GUARDIAN_PREFIX + guardian.SequenceOrder + ".json");
            await File.WriteAllTextAsync(filename, JsonConvert.SerializeObject(
                guardian,
                SerializationSettings.NewtonsoftSettings()));
        }
    }

    private static async Task ExportDevices(string deviceFolder, IEnumerable<EncryptionDevice> devices)
    {
        _ = Directory.CreateDirectory(deviceFolder);

        foreach (var device in devices)
        {
            var filename = Path.Combine(
                deviceFolder, DEVICE_PREFIX + device.DeviceUuid + ".json");
            await File.WriteAllTextAsync(filename, JsonConvert.SerializeObject(
                device,
                SerializationSettings.NewtonsoftSettings()));
        }
    }

    private static async Task ExportBallotsAsync(string ballotFolder, IEnumerable<CiphertextBallot> ballots)
    {
        _ = Directory.CreateDirectory(ballotFolder);

        foreach (var ballot in ballots)
        {
            var filename = Path.Combine(
                ballotFolder, ballot.ObjectId + ".json");
            await File.WriteAllTextAsync(filename, JsonConvert.SerializeObject(
                ballot,
                SerializationSettings.NewtonsoftSettings()));
        }
    }

    private static async Task ExportChallengedBallotsAsync(string challengedFolder, IEnumerable<PlaintextTallyBallot> ballots)
    {
        _ = Directory.CreateDirectory(challengedFolder);

        foreach (var ballot in ballots)
        {
            var filename = Path.Combine(
                challengedFolder, ballot.BallotId + ".json");
            await File.WriteAllTextAsync(filename, JsonConvert.SerializeObject(
                ballot,
                SerializationSettings.NewtonsoftSettings()));
        }
    }

    private static async Task ExportEncryptedTallyAsync(string filename, CiphertextTallyRecord tally)
    {
        await File.WriteAllTextAsync(filename, JsonConvert.SerializeObject(
            tally,
            SerializationSettings.NewtonsoftSettings()));
    }

    private static async Task ExportTallyAsync(string filename, PlaintextTally tally)
    {
        await File.WriteAllTextAsync(filename, JsonConvert.SerializeObject(
            tally,
            SerializationSettings.NewtonsoftSettings()));
    }

}
