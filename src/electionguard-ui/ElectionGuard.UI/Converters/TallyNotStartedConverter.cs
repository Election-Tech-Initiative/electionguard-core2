using System.Globalization;
using CommunityToolkit.Maui.Converters;

namespace ElectionGuard.UI.Converters;

public class TallyNotStartedConverter
    : TallyStateConverter
{
    public TallyNotStartedConverter() : base(TallyState.TallyStarted)
    {
    }
}
