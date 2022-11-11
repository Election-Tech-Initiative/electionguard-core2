using ElectionGuard.InteropGenerator.Helpers;

namespace ElectionGuard.InteropGenerator.Models;

public record EgProperty(
    string Name, 
    EgType Type,
    string Description
    )
{
    public string GetEntryPoint(string className)
    {
        return $"Eg{className}Get{Name}".ToSnakeCase();
    }

    public bool IsReferenceType()
    {
        return Type.IsReferenceType;
    }

    public string GetCReturnType()
    {
        return Type.GetCReturnType();
    }

    public string OutVarType => Type.OutVarCType;

    public bool CallerShouldFree => Type.TypeCs == "string";

    public bool IsElectionGuardType => Type.NativeHandleType != null;

    public bool IsPassByReference => Type.TypeCs == "string" || IsElectionGuardType;
}