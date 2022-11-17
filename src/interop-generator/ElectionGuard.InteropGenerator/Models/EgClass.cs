using ElectionGuard.InteropGenerator.Helpers;

namespace ElectionGuard.InteropGenerator.Models;

/// <summary>
/// 
/// </summary>
/// <param name="ClassName">The name of the class for C#.  The name in C will be derived unless CustomCName is specified.</param>
/// <param name="Properties">The class's properties.</param>
/// <param name="Methods">The class's methods</param>
/// <param name="CustomCName">If supplied this value will be used in handles and method names instead of ClassName.</param>
/// <param name="CInclude">An optional #include that will be generated for the C header file.</param>
/// <param name="NeverFree">If true a method will never free itself. This creates a memory leak, but is a required for some classes, see https://github.com/microsoft/electionguard-core2/issues/29</param>
public record EgClass(
    string ClassName,
    EgProperty[] Properties,
    EgMethod[] Methods,
    string? CustomCName,
    string? CInclude,
    bool NeverFree = false
)
{
    public string CFunctionPrefix => CustomCName ?? ClassName.ToSnakeCase();
}