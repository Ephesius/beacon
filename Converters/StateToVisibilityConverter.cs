using Beacon.Models;
using System.Globalization;

namespace Beacon.Converters;

public class StateToVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is AppState currentState && parameter is AppState targetState)
        {
            return currentState == targetState;
        }

        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}