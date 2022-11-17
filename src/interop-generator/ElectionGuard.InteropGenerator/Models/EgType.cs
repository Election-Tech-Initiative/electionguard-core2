namespace ElectionGuard.InteropGenerator.Models;

public record EgType(
    string TypeCs,
    string? TypeC,
    string? NativeHandleType
)
{
    private static readonly Dictionary<string, string> ReturnTypes = new()
    {
        { "string", "eg_electionguard_status_t" },
        { "bool", "bool" },
        { "ulong", "uint64_t" },
    };

    /// <summary>
    /// Returns the type of a variable in C assuming a caller will return the value (it will be an out parameter)
    /// </summary>
    public string OutVarCType
    {
        get
        {
            if (TypeCs == "string") return "char **";
            return TypeC + " **";
        }
    }

    private static readonly Dictionary<string, string> InParamTypes = new()
    {
        { "string", "char *" },
        { "bool", "bool " },
        { "ulong", "uint64_t " },
    };

    /// <summary>
    /// Returns the type of a variable in C assuming a caller will pass the data in.
    /// </summary>
    public string InVarCType
    {
        get
        {
            if (InParamTypes.TryGetValue(TypeCs, out var result))
                return result;
            return TypeC + " *";
        }
    }

    public bool IsElectionGuardType => NativeHandleType != null;

    public bool IsPassByReference => TypeCs == "string" || TypeCs == "byte[]" || IsElectionGuardType;

    public string GetCReturnType()
    {
        if (IsPassByReference) return "eg_electionguard_status_t";
        if (ReturnTypes.TryGetValue(TypeCs, out var value)) return value;
        if (TypeC != null) return TypeC;
        throw new NotSupportedException(
            $"{TypeCs} appears to be a value type that not yet supported.  Specify a TypeC or add a mapping to EgType.ReturnTypes.");
    }
}