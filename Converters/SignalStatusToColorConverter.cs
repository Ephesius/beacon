using Beacon.Models;
using System.Globalization;

namespace Beacon.Converters;

public class SignalStatusToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is GPSStatus status)
        {
            return status switch
            {
                GPSStatus.Strong => Colors.Green,
                GPSStatus.Weak => Colors.Yellow,
                GPSStatus.None => Colors.Red,
                _ => Colors.Gray
            };
        }

        return Colors.Gray;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}