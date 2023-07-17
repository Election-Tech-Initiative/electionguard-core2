using System.Globalization;
using CommunityToolkit.Maui.Converters;

namespace ElectionGuard.UI.Converters;

public class TallyPendingDecryptionsConverter
    : TallyStateConverter
{
    public TallyPendingDecryptionsConverter() : base(TallyState.PendingGuardianDecryptShares)
    {
    }

    public override bool ConvertFrom(TallyState value, CultureInfo? culture) => value == _state;
}
