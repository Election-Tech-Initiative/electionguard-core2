using CommandLine;
using ElectionGuard.CLI.Encrypt;

namespace ElectionGuard.CLI;

class Program
{
    static async Task Main(string[] args)
    {
        await Parser.Default.ParseArguments<EncryptOptions>(args)
            .WithParsedAsync(EncryptCommand.Encrypt);
    }
}