using ElectionGuard.InteropGenerator.Helpers;

namespace ElectionGuard.InteropGenerator.Models;

public record EgParam(
    string Name, 
    string TypeCs,
    string? DefaultValue
)
{
    public string TypeC => TypeHelper.CsToC(TypeCs);

    public string? MarshallAs()
    {
        return TypeCs switch
        {
            "string" => "UnmanagedType.LPStr",
            "ulong" => null,
            _ => throw new NotImplementedException("Unsupported marshall type " + TypeCs)
        };
    }

    public string AsCsParam()
    {
        var defaultVal = DefaultValue == null ? "" : $" = {DefaultValue}";
        return $"{TypeCs} {Name}{defaultVal}";
    }

    public string AsCppInteropParam()
    {
        var marshallAs = MarshallAs();
        var marshallAsAttribute = marshallAs == null ? "" : $"[MarshalAs({marshallAs})] ";
        return $"{marshallAsAttribute}{TypeCs} {Name}";
    }
}