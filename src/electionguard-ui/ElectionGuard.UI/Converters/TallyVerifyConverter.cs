using System.Globalization;

namespace ElectionGuard.UI.Converters;

internal class TallyVerifyConverter : TallyStateConverter
{
    public TallyVerifyConverter() : base(TallyState.AdminVerifyChallenge)
    {
    }

    public override bool ConvertFrom(TallyState value, CultureInfo? culture) => value == _state;
}
