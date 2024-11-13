using Beacon.Models;
using System.Globalization;

namespace Beacon.Converters;

/// <summary>
/// Converts GPS signal status to UI color values for visual status indication.
/// Follows the app's status communication standards for signal strength:
/// - Green: Strong signal (optimal accuracy)
/// - Yellow: Weak signal (degraded accuracy)
/// - Red: Not signal (Location unavailable)
/// - Gray: Unknown or uninitialized state
/// </summary>
public class SignalStatusToColorConverter : IValueConverter
{
    /// <summary>
    /// Converts a GPSStatus enum value to its corresponding Color.
    /// </summary>
    /// <param name="value">The GPSStatus enum value to convert</param>
    /// <param name="targetType">The type of the binding target property (unused)</param>
    /// <param name="parameter">Optional parameter (unused)</param>
    /// <param name="culture">Culture info (unused)</param>
    /// <returns>
    /// Colors.Green for Strong signal
    /// Colors.Yellow for Weak signal
    /// Colors.Red for No signal
    /// Colors.Gray for Unknown state
    /// </returns>
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

    /// <summary>
    /// Convert back method is not implemented as this is a one-way converter.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException">Always thrown when called</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}