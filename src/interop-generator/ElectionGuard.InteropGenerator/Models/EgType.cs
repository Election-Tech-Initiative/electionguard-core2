namespace ElectionGuard.InteropGenerator.Models;

public record EgType(
    string TypeCs,
    string? TypeC,
    string? NativeHandleType
)
{
    private static readonly string[] ValueTypes = {
        "bool", "ulong"
    };

    private static readonly Dictionary<string, string> ReturnTypes = new()
    {
        { "string", "eg_electionguard_status_t" },
        { "bool", "bool" },
        { "ulong", "uint64_t" },
    };

    public bool IsReferenceType => !ValueTypes.Contains(TypeCs);

    public string OutVarCType
    {
        get
        {
            if (TypeCs == "string") return "char **";
            return TypeC + " **";
        }
    }

    public bool IsElectionGuardType => NativeHandleType != null;

    public bool IsPassByReference => TypeCs == "string" || TypeCs == "byte[]" || IsElectionGuardType;

    public string GetCReturnType()
    {
        ReturnTypes.TryGetValue(TypeCs, out var value);
        return value ?? "eg_electionguard_status_t";
    }
}