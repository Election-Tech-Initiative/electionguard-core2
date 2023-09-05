using CommandLine;
using ElectionGuard.CLI.Encrypt;
using ElectionGuard.CLI.Generate;
using System.Reflection;
using ElectionGuard.Converters;
using Newtonsoft.Json;

namespace ElectionGuard.CLI;

class Program
{
    static async Task Main(string[] args)
    {
        JsonConvert.DefaultSettings = SerializationSettings.NewtonsoftSettings;
        var verbs = LoadVerbs();
        _ = await Parser.Default.ParseArguments(args, verbs)
            .WithParsedAsync(Run);
    }

    //load all types using Reflection
    private static Type[] LoadVerbs()
    {
        return Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.GetCustomAttribute<VerbAttribute>() != null).ToArray();
    }

    private static async Task Run(object obj)
    {
        switch (obj)
        {
            case CreateElectionOptions c:
                await CreateElectionCommand.Execute(c);
                break;
            case EncryptOptions o:
                await EncryptCommand.Execute(o);
                break;
            case GenerateOptions g:
                await GenerateCommand.Execute(g);
                break;
            case VerifyOptions v:
                await VerifyCommand.Execute(v);
                break;
        }
    }
}
