using System;
using System.IO;

namespace ElectionGuard.CLI.Encrypt
{
    /// <summary>
    /// Create an Election Package.
    /// </summary>
    internal class CreateElectionCommand
    {
        public static Task Execute(CreateElectionOptions options)
        {
            try
            {
                var command = new CreateElectionCommand();
                return command.ExecuteInternal(options);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private Task ExecuteInternal(CreateElectionOptions options)
        {
            Console.WriteLine("Create Election");

            options.Validate();

            if (string.IsNullOrEmpty(options.Manifest))
            {
                throw new ArgumentNullException(nameof(options.Manifest));
            }

            Console.WriteLine($"Loading Manifest {options.Manifest}");

            // load the manifest
            var manifestJson = File.ReadAllText(options.Manifest);

            Console.WriteLine("Loading Manifest");

            var manifest = new Manifest(manifestJson);

            Console.WriteLine("Loading Internal Manifest");

            var internalManifest = new InternalManifest(manifest);

            Console.WriteLine("Loading Context");

            var commitmentHash = new ElementModQ(options.CommitmentHash);
            var publicKey = new ElementModP(options.ElGamalPublicKey);

            var context = new CiphertextElectionContext(
                (ulong)options.NumberOfGuardians, (ulong)options.Quorum,
                publicKey, commitmentHash, internalManifest.ManifestHash);

            Console.WriteLine("Saving Files");

            // write the files to the output directory
            File.WriteAllText(Path.Join(options.OutDir, "context.json"), context.ToJson());
            File.WriteAllText(Path.Join(options.OutDir, "manifest.json"), manifest.ToJson());


            Console.WriteLine("Create Election Complete");
            return Task.CompletedTask;
        }
    }
}
