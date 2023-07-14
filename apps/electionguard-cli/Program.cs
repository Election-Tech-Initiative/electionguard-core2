﻿using CommandLine;
using ElectionGuard.CLI.Encrypt;
using ElectionGuard.CLI.Generate;
using System.Reflection;

namespace ElectionGuard.CLI;

class Program
{
    static async Task Main(string[] args)
    {
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
        }
    }
}
