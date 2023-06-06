using System.Globalization;
using CommunityToolkit.Maui.Converters;

namespace ElectionGuard.UI.Converters;

internal class TallyCanBeAbandonedConverter
    : BaseConverter<TallyState, bool,bool>
{
    public override bool DefaultConvertReturnValue { get; set; }
    public override TallyState DefaultConvertBackReturnValue { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public override TallyState ConvertBackTo(bool value, bool parameter, CultureInfo? culture)
    {
        throw new NotImplementedException();
    }

    public override bool ConvertFrom(TallyState value, bool isAdmin,  CultureInfo? culture) => isAdmin && value < TallyState.TallyStarted;
}
