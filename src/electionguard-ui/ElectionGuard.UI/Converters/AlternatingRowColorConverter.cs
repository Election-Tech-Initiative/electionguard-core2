using System.Globalization;

namespace ElectionGuard.UI.Converters;

public class AlternateRowColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var collectionView = parameter as CollectionView;
        var index = collectionView.ItemsSource.Cast<object>().ToList().IndexOf(value);

        if (index % 2 == 0)
        {
            // Even rows use the default color
            return Color.FromArgb("#FFE1E1E1");
        }
        else
        {
            // Odd rows use the alternate color
            return Color.FromArgb("#FFC8C8C8");
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
