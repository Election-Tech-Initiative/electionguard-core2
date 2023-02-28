using System.Globalization;

namespace ElectionGuard.UI.Helpers;

class BadVerifyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var badVerify = (bool)value;
        return badVerify ? "badverify.png" : "verify.png";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

class BadVerifyTooltipConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var badVerify = (bool)value;
        return badVerify ? "Bad Verification\nContact the election administrator!" : "Verified";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
