using ElectionGuard.InteropGenerator.Helpers;

namespace ElectionGuard.InteropGenerator.Models;

public record EgMethod (
    string Name,
    EgType ReturnType,
    string Description,
    EgParam[] Params,
    bool? CallerShouldFree)
{
    public string GetEntryPoint(string className)
    {
        return $"Eg{className}{Name}".ToSnakeCase();
    }

    public string GetCReturnType()
    {
        return ReturnType.GetCReturnType();
    }

    public string ReturnTypeCName => "out_" + Name.ToSnakeCase() + "_ref";
}
