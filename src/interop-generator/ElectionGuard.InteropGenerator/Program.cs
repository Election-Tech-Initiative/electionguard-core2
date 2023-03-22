/*
 * To generate the proxy code for ElectionGuard
 *
 * 1.  Modify EgInteropClasses.json, then either:
 * 2a. Run this from Visual Studio via F5, or
 * 2b. From project root `make generate-interop`, or
 * 2c. dotnet run -- [InteropJsonPath][ProjectRootDir]
 * 3.  Review source control changes very carefully
 * 4.  Delete any newly created dead code, starting with NativeInterface and working out
 * 5.  Try to compile
 * 6.  Rerun all unit tests
 *
 * Tips:
 * - Make small changes
 * - Start with C# then move to C
 * - Unfortunately you'll need Visual Studio if you need to change the .tt files
 * - When working on a mac, you might have to manually configure the template generation in visual sudio for mac
 * - see: https://stackoverflow.com/a/68413028
 */

using ElectionGuard.InteropGenerator.FileGenerators;

var egInteropClassesGenerator = new InteropGenerator();
var interopJsonPath = GetInteropJsonPath(args);
SetProjectRootDir(args);
await egInteropClassesGenerator.GenerateAll(interopJsonPath);

static string GetInteropJsonPath(string[] args)
{
    var firstArg = args.Length >= 1 ? args[0] : null;
    return firstArg ?? Path.Join(Directory.GetCurrentDirectory(), "/EgInteropClasses.json");
}

static void SetProjectRootDir(string[] args)
{
    if (args.Length >= 2)
    {
        FileGeneratorBase.ProjectRootDir = args[1];
    }
}
