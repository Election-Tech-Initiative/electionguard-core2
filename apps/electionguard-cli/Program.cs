using CommandLine;
using ElectionGuard.CLI.Encrypt;
using System;
using System.Reflection;

namespace ElectionGuard.CLI;

class Program
{
    static async Task Main(string[] args)
    {
        var verbs = LoadVerbs();
        await Parser.Default.ParseArguments(args, verbs)
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
                CreateElectionCommand.Execute(c);
                break;
            case EncryptOptions o:
                EncryptCommand.Execute(o);
                break;
        }
    }
}
