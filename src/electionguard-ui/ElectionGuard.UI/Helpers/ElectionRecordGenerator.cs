using System.IO.Compression;
using System.Text.Json;
using CommunityToolkit.Mvvm.DependencyInjection;
using ElectionGuard.ElectionSetup;
using ElectionGuard.UI.Lib.Services;
using MongoDB.Driver;

namespace ElectionGuard.UI.Helpers;

public static class ElectionRecordGenerator
{
    private static readonly string GUARDIAN_FOLDER = "guardians";
    private static readonly string DEVICE_FOLDER = "encryption_devices";
    private static readonly string BALLOT_FOLDER = "submitted_ballots";

    private static readonly string GUARDIAN_PREFIX = "guardian_";
    private static readonly string DEVICE_PREFIX = "device_";


    public static async Task GenerateEelectionRecord(TallyRecord tally, string outputFolder)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(outputFolder));
        
        // check the state of the tally
        if (tally.State != TallyState.Complete)
        {
            throw new ArgumentException("The tally must be completed before generating an election record.");
        }

        // generate temp path to export all of the files to
        var tempFolder = Directory.CreateTempSubdirectory("eg_");

        // export the guardians
        await ExportGuardiansAsync(Path.Combine(tempFolder.FullName, GUARDIAN_FOLDER), tally.KeyCeremonyId!);

        // export the devices
        await ExportDevices(Path.Combine(tempFolder.FullName, DEVICE_FOLDER), tally.ElectionId!);

        // export the ballots
        await ExportBallotsAsync(Path.Combine(tempFolder.FullName, BALLOT_FOLDER), tally.ElectionId!);

        // export the top level
        await ExportSummaryAsync(tempFolder.FullName, tally.ElectionId!, tally.TallyId);

        // create the zip from the folder
        var zipFile = Path.Combine(outputFolder, tally.Name!.Replace(" ", "_") + ".zip");
        ZipFile.CreateFromDirectory(tempFolder.FullName, zipFile);

        // delete the temp folder1
        Directory.Delete(tempFolder.FullName, true);
    }

    private static async Task ExportGuardiansAsync(string guardianFolder, string keyCeremonyId)
    {
        Directory.CreateDirectory(guardianFolder);

        // get all of the guardians public keys using key ceremony id
        GuardianPublicKeyService guardianPublicKeyService = new();
        var guardians = await guardianPublicKeyService.GetAllByKeyCeremonyIdAsync(keyCeremonyId);
        foreach (var guardian in guardians)
        {
            await File.WriteAllTextAsync(Path.Combine(guardianFolder, $"{GUARDIAN_PREFIX}{guardian.GuardianId}.json"), JsonSerializer.Serialize(guardian.PublicKey));
        }
    }

    private static async Task ExportDevices(string deviceFolder, string electionId)
    {
        Directory.CreateDirectory(deviceFolder);

        // loop thru all ballot exports
        BallotUploadService ballotUploadService = new();
        var ballotUploads = await ballotUploadService.GetByElectionIdAsync(electionId);
        // export device file
        foreach (var upload in ballotUploads)
        {
            using var device = new EncryptionDevice((ulong)upload.DeviceId, (ulong)upload.SessionId, (ulong)upload.LaunchCode, upload.Location);
            await File.WriteAllTextAsync(Path.Combine(deviceFolder, $"{DEVICE_PREFIX}{upload.DeviceId}.json"), device.ToJson());
        }
    }

    private static async Task ExportBallotsAsync(string ballotFolder, string electionId)
    {
        Directory.CreateDirectory(ballotFolder);

        BallotService ballotService = new();
        using var cursor = await ballotService.GetCursorByElectionIdAsync(electionId);

        await cursor.ForEachAsync(document =>
        {
            var ballotCode = document.BallotCode;
            var fileName = $"{ballotCode}.json";
            var filePath = Path.Combine(ballotFolder, fileName);

            using var writer = new StreamWriter(filePath);
            var json = document.BallotData;
            writer.WriteLine(json);
        });
    }

    private static async Task ExportSummaryAsync(string summaryFolder, string electionId, string tallyId)
    {
        // write manifest
        ManifestService manifestService = new();
        var manifest = await manifestService.GetByElectionIdAsync(electionId);
        await File.WriteAllTextAsync(Path.Combine(summaryFolder, "manifest.json"), manifest);

        // write context
        ContextService contextService = new();
        var context = await contextService.GetByElectionIdAsync(electionId);
        await File.WriteAllTextAsync(Path.Combine(summaryFolder, "context.json"), context);

        // write constants
        ConstantsService constantsService = new();
        var constants = await constantsService.GetByElectionIdAsync(electionId);
        await File.WriteAllTextAsync(Path.Combine(summaryFolder, "constants.json"), constants);

        // write coefficients
        LagrangeCoefficientsService lagrangeCoefficientsService = new();
        var coefficients = await lagrangeCoefficientsService.GetByTallyIdAsync(tallyId);
        await File.WriteAllTextAsync(Path.Combine(summaryFolder, "coefficients.json"), coefficients);

        // write plaintext tally
        PlaintextTallyService plaintextTallyService = new();
        var plaintextTally = await plaintextTallyService.GetByTallyIdAsync(tallyId);
        await File.WriteAllTextAsync(Path.Combine(summaryFolder, "tally.json"), plaintextTally);

        // write encrypted tally
        EncryptedTallyService encryptedTallyService = new();
        var encryptedTally = await encryptedTallyService.GetByTallyIdAsync(tallyId);
        await File.WriteAllTextAsync(Path.Combine(summaryFolder, "encrypted_tally.json"), encryptedTally);
    }
}
