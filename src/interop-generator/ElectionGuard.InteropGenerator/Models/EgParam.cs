using ElectionGuard.InteropGenerator.Helpers;

namespace ElectionGuard.InteropGenerator.Models;

public record EgParam(
    string Name, 
    EgType Type,
    string? DefaultValue
)
{
    public string TypeC => TypeHelper.CsToC(Type.TypeCs);

    public string? MarshallAs()
    {
        return Type.TypeCs switch
        {
            "string" => "UnmanagedType.LPStr",
            "ulong" => null,
            _ => throw new NotImplementedException("Unsupported marshall type " + Type.TypeCs)
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
        return $"{marshallAsAttribute}{Type.TypeCs} {Name}";
    }
}