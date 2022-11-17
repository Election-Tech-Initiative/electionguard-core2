using ElectionGuard.InteropGenerator.Helpers;

namespace ElectionGuard.InteropGenerator.Models;

public record EgParam(
    string Name, 
    EgType Type,
    string? DefaultValue,
    string? Description
)
{
    public string TypeC => Type.InVarCType;

    public string? MarshallAs()
    {
        return Type.TypeCs switch
        {
            "string" => "UnmanagedType.LPStr",
            "ulong" => null,
            _ => null
        };
    }

    public string AsCsParam()
    {
        var defaultVal = DefaultValue == null ? "" : $" = {DefaultValue}";
        return $"{Type.TypeCs} {Name}{defaultVal}";
    }

    public string AsCppInteropParam()
    {
        var marshallAs = MarshallAs();
        var marshallAsAttribute = marshallAs == null ? "" : $"[MarshalAs({marshallAs})] ";
        return $"{marshallAsAttribute}{Type.NativeHandleType ?? Type.TypeCs} {Name}";
    }

    public string CName => $"in_{Name.ToSnakeCase()}";

    public bool IsElectionGuardType => Type.IsElectionGuardType;

    public string ToCsArgument()
    {
        return IsElectionGuardType ? $"{Name}.Handle" : Name;
    }
}
