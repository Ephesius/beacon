using Beacon.Models;
using System.Globalization;

namespace Beacon.Converters;

public class SignalStatusToEnabledConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is GPSStatus status)
        {
            return status == GPSStatus.Strong;
        }

        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
