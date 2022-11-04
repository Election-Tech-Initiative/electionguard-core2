using ElectionGuard.InteropGenerator.Models;

namespace ElectionGuard.InteropGenerator.FileGenerators;

public abstract class FileGeneratorBase
{
    public static string ProjectRootDir = "../../../../../..";

    public abstract GeneratedClass Generate(EgClass egClass);
}
