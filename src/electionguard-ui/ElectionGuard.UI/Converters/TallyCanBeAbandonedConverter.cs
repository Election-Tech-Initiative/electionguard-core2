using System.Globalization;
using CommunityToolkit.Maui.Converters;

namespace ElectionGuard.UI.Converters;

public class TallyCanBeAbandonedConverter
    : TallyStateConverter
{
    public TallyCanBeAbandonedConverter() : base(TallyState.TallyStarted)
    {
    }
}
