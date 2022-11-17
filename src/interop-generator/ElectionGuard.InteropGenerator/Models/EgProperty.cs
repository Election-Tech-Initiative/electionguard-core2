using ElectionGuard.InteropGenerator.Helpers;
using System.Security.Cryptography;

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

    /// <summary>
    /// Returns the type that should be returned in C# by the extern method in the External class.
    /// </summary>
    public string GetExternalReturnType()
    {
        if (IsPassByReference)
            return "Status";
        if (Type.TypeCs == "DateTime")
            return "ulong";
        return Type.TypeCs;
    }

    public string OutVarType => Type.OutVarCType;

    public bool CallerShouldFree => Type.TypeCs == "string";

    public bool IsElectionGuardType => Type.IsElectionGuardType;

    public bool IsPassByReference => Type.IsPassByReference;
}