using ElectionGuard.Encryption.Utils.Generators;
using ElectionGuard.Extensions;
using Newtonsoft.Json;

namespace ElectionGuard.CLI.Generate
{
    /// <summary>
    /// Generate plaintext ballots from a manifest and encrypt them using a context.
    /// </summary>
    internal class GenerateCommand
    {
        public static Task Execute(GenerateOptions options)
        {
            try
            {
                var command = new GenerateCommand();
                return command.ExecuteInternal(options);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private async Task ExecuteInternal(GenerateOptions options)
        {
            options.Validate();

            PlainTally? tally = null;
            var context = Path.Combine(options.WorkingDir, "context.json");
            var manifest = Path.Combine(options.WorkingDir, "manifest.json");
            using var internalManifest = GetInternalManifest(manifest);
            using var encryptionMediator = GetEncryptionMediator(context, manifest);

            var plaintextBallots = BallotGenerator.GetFakeBallots(
                internalManifest, Random.Shared, options.BallotCount);

            var plaintextPath = Path.Combine(options.WorkingDir, "plaintext");
            var encryptedPath = Path.Combine(options.WorkingDir, "encrypted_ballots");

            foreach (var ballot in plaintextBallots)
            {
                var ciphertext = encryptionMediator.Encrypt(ballot, false);
                if (Random.Shared.NextSingle() > options.SpoiledPercent / 100.0)
                {
                    ciphertext.Cast();
                }
                else
                {
                    ciphertext.Challenge();
                }
                var ballotCode = ciphertext.BallotCode;
                var ballotData = ciphertext.ToJson();
                File.WriteAllText(Path.Combine(encryptedPath, $"{ballotCode}.json"), ballotData);
                var data = ballot.ToJson();

                if (tally == null)
                {
                    tally = JsonConvert.DeserializeObject<PlainTally>(data);
                    tally?.Clear();
                    tally.object_id = $"Tally for {plaintextBallots.Count} ballots";
                    tally.style_id = string.Empty;
                }
                if (ciphertext.IsCast)
                {
                    var b = JsonConvert.DeserializeObject<PlainTally>(data);
                    tally += b;
                }
                if (options.PlaintextOutput)
                {
                    File.WriteAllText(Path.Combine(plaintextPath, $"{ballotCode}.json"), data);
                }
                Console.WriteLine($"Generated Ballot {ballotCode}.json");
                ciphertext.Dispose();
            }
            var tallyJson = JsonConvert.SerializeObject(tally);
            File.WriteAllText(Path.Combine(options.WorkingDir, $"tally.json"), tallyJson);

            plaintextBallots.Dispose();
            encryptionMediator.Dispose();

            Console.WriteLine("Generation Complete");
        }

        private static EncryptionMediator GetEncryptionMediator(string contextFile, string manifestFile)
        {
            using var context = GetContext(contextFile);
            using var internalManifest = GetInternalManifest(manifestFile);
            using var device = GetDevice();
            var encryptionMediator = new EncryptionMediator(internalManifest, context, device);
            return encryptionMediator;
        }

        private static EncryptionDevice GetDevice()
        {
            return new EncryptionDevice(12345UL, 23456UL, 34567UL, "Location");
        }

        private static InternalManifest GetInternalManifest(string manifestFile)
        {
            var manifestJson = File.ReadAllText(manifestFile);
            using var manifest = new Manifest(manifestJson);
            return new InternalManifest(manifest);
        }

        private static CiphertextElectionContext GetContext(string contextFile)
        {
            var contextJson = File.ReadAllText(contextFile);
            return new CiphertextElectionContext(contextJson);
        }
    }
}
