using System.Globalization;
using CommunityToolkit.Maui.Converters;

namespace ElectionGuard.UI.Converters;

internal class TallyNotStartedConverter
    : BaseConverter<TallyState, bool>
{
    public override bool DefaultConvertReturnValue { get; set; }
    public override TallyState DefaultConvertBackReturnValue { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public override TallyState ConvertBackTo(bool value, CultureInfo? culture) => throw new NotImplementedException();
    public override bool ConvertFrom(TallyState value, CultureInfo? culture) => value < TallyState.TallyStarted;
}
