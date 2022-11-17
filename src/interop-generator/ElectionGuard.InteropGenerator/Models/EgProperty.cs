using ElectionGuard.InteropGenerator.Helpers;

namespace ElectionGuard.InteropGenerator.Models;

public record EgProperty(
    string Name, 
    EgType Type,
    string Description
    )
{
    public string GetEntryPoint(EgClass egClass)
    {
        return $"eg_{egClass.CFunctionPrefix}Get{Name}".ToSnakeCase();
    }

    public string GetCReturnType()
    {
        return Type.GetCReturnType();
    }

    public string OutVarType => Type.OutVarCType;

    public bool CallerShouldFree => Type.TypeCs == "string";

    public bool IsElectionGuardType => Type.IsElectionGuardType;

    public bool IsPassByReference => Type.IsPassByReference;
}