using Beacon.Models;
using System.Globalization;

namespace Beacon.Converters;

/// <summary>
/// Converts GPS signal status to boolean values for controlling UI element enabled states.
/// Primary use is for controlling action action availability based on GPS signal quality.
/// 
/// Key usage: "Set Beacon" button is only enabled with strong GPS signal to ensure
/// accurate location capture.
/// </summary>
public class SignalStatusToEnabledConverter : IValueConverter
{
    /// <summary>
    /// Converts a GPSStatus enum value to a boolean indicating if the associated UI element should be enabled.
    /// </summary>
    /// <param name="value">The GPSStatus enum value to convert</param>
    /// <param name="targetType">The type of the binding target property (unused)</param>
    /// <param name="parameter">Optional parameter (unused)</param>
    /// <param name="culture">Culture info (unused)</param>
    /// <returns>
    /// true: When GPS signal is Strong
    /// false: When GPS signal is Weak or None, or for invalid/null input
    /// </returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is GPSStatus status)
        {
            return status == GPSStatus.Strong;
        }

        return false;
    }

    /// <summary>
    /// Convert back method is not implemented as this is a one-way converter.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException">Always thrown when called</exception>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
