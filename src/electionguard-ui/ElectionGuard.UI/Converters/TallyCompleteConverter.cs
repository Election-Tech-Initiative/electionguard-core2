using System.Globalization;
using CommunityToolkit.Maui.Converters;

namespace ElectionGuard.UI.Converters;

public class TallyCompleteConverter
    : TallyStateConverter
{
    public TallyCompleteConverter() : base(TallyState.Complete)
    {
    }

    public override bool ConvertFrom(TallyState value, CultureInfo? culture) => value == _state;
}
