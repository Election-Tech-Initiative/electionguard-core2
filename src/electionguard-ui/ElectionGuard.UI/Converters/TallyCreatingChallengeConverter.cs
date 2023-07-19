using System.Globalization;

namespace ElectionGuard.UI.Converters;

public class TallyCreatingChallengeConverter
    : TallyStateConverter
{
    public TallyCreatingChallengeConverter() : base(TallyState.AdminGenerateChallenge)
    {
    }

    public override bool ConvertFrom(TallyState value, CultureInfo? culture) => value == _state;
}
