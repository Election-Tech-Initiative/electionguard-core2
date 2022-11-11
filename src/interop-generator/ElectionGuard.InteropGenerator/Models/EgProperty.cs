using ElectionGuard.InteropGenerator.Helpers;

namespace ElectionGuard.InteropGenerator.Models;

public record EgProperty(
    string Name, 
    EgType Type,
    string Description
    )
{
    private static readonly Dictionary<string, string> ReturnTypes = new()
    {
        { "string", "eg_electionguard_status_t" },
        { "bool", "bool" },
        { "ulong", "uint64_t" },
    };

    private static readonly string[] ValueTypes = {
        "bool", "ulong"
    };

    public string GetEntryPoint(string className)
    {
        return $"Eg{className}Get{Name}".ToSnakeCase();
    }

    public bool IsReferenceType()
    {
        return !ValueTypes.Contains(Type.TypeCs);
    }

    public string GetCReturnType()
    {
        ReturnTypes.TryGetValue(Type.TypeCs, out var value);
        return value ?? "eg_electionguard_status_t";
    }

    public string GetOutVarType()
    {
        if (Type.TypeCs == "string") return "char **";
        return Type.TypeC + " **";
    }

    public bool CallerShouldFree => Type.TypeCs == "string";

    public bool IsElectionGuardType => Type.NativeHandleType != null;

    public bool IsPassByReference => Type.TypeCs == "string" || IsElectionGuardType;
}