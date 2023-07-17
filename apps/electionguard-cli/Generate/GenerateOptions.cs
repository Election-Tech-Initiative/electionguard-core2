using CommandLine;

namespace ElectionGuard.CLI.Generate;

[Verb("generate", HelpText = "Generate plaintext ballots from a manifest and encrypt them using a context.")]
internal class GenerateOptions
{
    [Option('f', "folder", Required = true, HelpText = "File folder to read and write data into.")]
    public string WorkingDir { get; set; } = string.Empty;

    [Option('p', "plain", Required = false, HelpText = "Setting to save the plaintext ballots as well as the encrypted ones.")]
    public bool PlaintextOutput { get; set; } = false;

    [Option('s', "spoiled_percent", Required = false, HelpText = "Percentage to automatically spoil ballots during encryption.  Defaults to 0.0")]
    public double SpoiledPercent { get; set; } = 0.0;

    [Option('c', "count", Required = false, HelpText = "Number of ballots to generate and encrypt. Defaults to 100.")]
    public int BallotCount { get; set; } = 100;

    public void Validate()
    {
        ValidateDirectories();
        ValidateFiles();
    }

    private void ValidateDirectories()
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(WorkingDir));

        if (PlaintextOutput)
        {
            var plaintextPath = Path.Combine(WorkingDir, "plaintext");
            Console.WriteLine($"Creating plaintext directory: {plaintextPath}");
            Directory.CreateDirectory(plaintextPath);
        }

        var encryptedPath = Path.Combine(WorkingDir, "encrypted");
        Console.WriteLine($"Creating encrypted directory: {encryptedPath}");
        Directory.CreateDirectory(encryptedPath);
    }

    private void ValidateFiles()
    {
        var requiredFiles = new[] { "context.json", "manifest.json" };
        var missingFiles = requiredFiles.Where(f => !File.Exists(Path.Combine(WorkingDir, f)));
        foreach (var file in missingFiles)
        {
            throw new ArgumentException($"{file} does not exist");
        }
    }
}
