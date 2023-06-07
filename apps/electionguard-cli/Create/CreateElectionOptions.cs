using CommandLine;

namespace ElectionGuard.CLI.Encrypt;

[Verb("create-election", HelpText = "Create an Election Package.")]
internal class CreateElectionOptions
{
    [Option('c', "commitment", Required = true, HelpText = "The commitment hash that guardians make to each other to complete the key ceremony.")]
    public string CommitmentHash { get; set; }

    [Option('m', "manifest", Required = true, HelpText = "Json file containing an ElectionGuard manifest that contains election details.")]
    public string Manifest { get; set; }

    [Option('g', "guardians", Required = true, HelpText = "The number of Guardians")]
    public int NumberOfGuardians { get; set; }

    [Option('q', "quorum", Required = true, HelpText = "The Quorum of guardians")]
    public int Quorum { get; set; }

    [Option('k', "publicKey", Required = true, Separator = ',', HelpText = "Elgamal public key of the key ceremony")]
    public string ElGamalPublicKey { get; set; }

    [Option('o', "out", Required = true, HelpText = "File folder in which to place encryption package.")]
    public string? OutDir { get; set; }

    public void Validate()
    {
        if (Quorum > NumberOfGuardians)
        {
            throw new ArgumentException("Quorum cannot be greater than the number of guardians");
        }
        ValidateDirectories();
        ValidateFiles();
    }

    private void ValidateDirectories()
    {
        if (string.IsNullOrEmpty(OutDir))
            throw new ArgumentNullException(nameof(OutDir));

        if (!Directory.Exists(OutDir))
        {
            Console.WriteLine($"Creating directory: {OutDir}");
            Directory.CreateDirectory(OutDir);
        }
    }

    private void ValidateFiles()
    {
        var requiredFiles = new[] { Manifest };
        var missingFiles = requiredFiles.Where(f => !File.Exists(f));
        foreach (var file in missingFiles)
        {
            throw new ArgumentException($"{file} does not exist");
        }
    }
}
