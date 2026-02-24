namespace ElectionGuard.UI.Lib.Models;

public partial class LagrangeCoefficientsRecord : DatabaseRecord
{
    [ObservableProperty]
    private string _tallyId = string.Empty;

    [ObservableProperty]
    private string? _lagrangeCoefficientsData;

    public LagrangeCoefficientsRecord() : base(nameof(LagrangeCoefficientsRecord))
    {
    }

    public override string ToString() => LagrangeCoefficientsData ?? string.Empty;
    public static implicit operator string(LagrangeCoefficientsRecord? record) => record?.ToString() ?? string.Empty;
}
