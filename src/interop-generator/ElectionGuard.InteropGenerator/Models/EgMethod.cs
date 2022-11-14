using ElectionGuard.InteropGenerator.Helpers;

namespace ElectionGuard.InteropGenerator.Models;

public record EgMethod (
    string Name,
    EgType ReturnType,
    string Description,
    EgParam[] Params)
{
    public string ReturnTypeC => TypeHelper.CsToC(ReturnType.TypeCs);
    
    public string GetEntryPoint(string className)
    {
        return $"Eg{className}{Name}".ToSnakeCase();
    }
}
