using System.Globalization;

namespace ElectionGuard.UI.Converters;

public class TallyAccumulatingConverter
    : TallyStateConverter
{
    public TallyAccumulatingConverter() : base(TallyState.AdminAccumulateTally)
    {
    }

    public override bool ConvertFrom(TallyState value, CultureInfo? culture) => value == _state;
}
