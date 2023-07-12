using System.Globalization;

namespace ElectionGuard.UI.Converters;

public class MultiBackgroundColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool multi = (bool)value;
        return multi ? Color.FromArgb("#FF70cab8") : Color.FromArgb("#FF409388");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
