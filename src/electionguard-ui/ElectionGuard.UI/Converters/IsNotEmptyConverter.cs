using System.Globalization;
using CommunityToolkit.Maui.Converters;

namespace ElectionGuard.UI.Converters;

internal class IsNotEmptyConverter : BaseConverter<string, bool>
{
    public override bool DefaultConvertReturnValue { get; set; }

    public override string DefaultConvertBackReturnValue { get; set; } = string.Empty;

    public override string ConvertBackTo(bool value, CultureInfo? culture) => throw new NotImplementedException();

    public override bool ConvertFrom(string value, CultureInfo? culture) => culture != null && !value.Equals(string.Empty);
}
