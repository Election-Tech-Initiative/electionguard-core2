namespace ElectionGuard.CLI.Encrypt
{
    internal class EncryptCommand
    {
        public static Task Execute(EncryptOptions options)
        {
            try
            {
                var command = new EncryptCommand();
                return command.ExecuteInternal(options);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private async Task ExecuteInternal(EncryptOptions options)
        {
            options.Validate();
            if (string.IsNullOrEmpty(options.Context))
                throw new ArgumentNullException(nameof(options.Context));
            if (string.IsNullOrEmpty(options.Manifest))
                throw new ArgumentNullException(nameof(options.Manifest));
            if (string.IsNullOrEmpty(options.BallotsDir))
                throw new ArgumentNullException(nameof(options.BallotsDir));

            var encryptionMediator = await GetEncryptionMediator(
                options.Context, options.Manifest);
            var ballotFiles = GetBallotFiles(options.BallotsDir);

            foreach (var ballotFile in ballotFiles)
            {
                Console.WriteLine($"Parsing: {ballotFile}");
                var plaintextBallot = await GetPlaintextBallot(ballotFile);
                var spoiledDeviceIds = options.SpoiledDeviceIds.ToList();
                var submittedBallot = EncryptAndSubmit(
                    encryptionMediator, plaintextBallot, spoiledDeviceIds, ballotFile);
                await WriteSubmittedBallot(options, ballotFile, submittedBallot);
            }
            Console.WriteLine("Parsing Complete");
        }

        private static SubmittedBallot EncryptAndSubmit(EncryptionMediator encryptionMediator,
            PlaintextBallot plaintextBallot, IList<string> spoiledDeviceIds, string ballotFile)
        {
            var ciphertextBallot = encryptionMediator.Encrypt(plaintextBallot);
            var shouldSpoil = spoiledDeviceIds.Contains(
                ciphertextBallot.ObjectId, StringComparer.OrdinalIgnoreCase) ||
                              spoiledDeviceIds.Contains(
                                Path.GetFileNameWithoutExtension(ballotFile), StringComparer.OrdinalIgnoreCase);
            var state = shouldSpoil ? BallotBoxState.Spoiled : BallotBoxState.Cast;
            var submittedBallot = new SubmittedBallot(ciphertextBallot, state);
            return submittedBallot;
        }

        private static async Task<EncryptionMediator> GetEncryptionMediator(
            string contextFile, string manifestFile)
        {
            var context = await GetContext(contextFile);
            var internalManifest = await GetInternalManifest(manifestFile);
            var device = GetDevice();
            var encryptionMediator = new EncryptionMediator(internalManifest, context, device);
            return encryptionMediator;
        }

        private static IEnumerable<string> GetBallotFiles(string directory)
        {
            return Directory.EnumerateFiles(directory);
        }

        private static async Task<PlaintextBallot> GetPlaintextBallot(string ballotFile)
        {
            var ballot = await File.ReadAllTextAsync(ballotFile);
            var plaintextBallot = new PlaintextBallot(ballot);
            return plaintextBallot;
        }

        private static async Task WriteSubmittedBallot(EncryptOptions encryptOptions, string ballotFile,
            SubmittedBallot submittedBallot)
        {
            var ballotJson = submittedBallot.ToJson();
            var outFile = Path.Join(encryptOptions.OutDir, Path.GetFileName(ballotFile));
            await File.WriteAllTextAsync(outFile, ballotJson);
        }

        private static EncryptionDevice GetDevice()
        {
            return new EncryptionDevice(12345UL, 23456UL, 34567UL, "Location");
        }

        private static async Task<InternalManifest> GetInternalManifest(string manifestFile)
        {
            var manifestJson = await File.ReadAllTextAsync(manifestFile);
            var manifest = new Manifest(manifestJson);
            return new InternalManifest(manifest);
        }

        private static async Task<CiphertextElectionContext> GetContext(string contextFile)
        {
            var contextJson = await File.ReadAllTextAsync(contextFile);
            return new CiphertextElectionContext(contextJson);
        }
    }
}
