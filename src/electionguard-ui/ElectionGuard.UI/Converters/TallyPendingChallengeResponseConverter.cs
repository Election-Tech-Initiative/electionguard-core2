using System.Globalization;

namespace ElectionGuard.UI.Converters;

public class TallyPendingChallengeResponseConverter
    : TallyStateConverter
{
    public TallyPendingChallengeResponseConverter() : base(TallyState.PendingGuardianRespondChallenge)
    {
    }

    public override bool ConvertFrom(TallyState value, CultureInfo? culture) => value == _state;
}
