using Beacon.Models;
using System.Globalization;

namespace Beacon.Converters;

/// <summary>
/// Converts application state to visibility values for UI elements.
/// Controls the visibility of state-specific UI elements.
/// Each app state (SetBeacon, FindBeacon, Navigation, Destination)
/// has a unique set of visible elements.
/// 
/// Usage example in XAML:
/// IsVisible="{Binding CurrentState, Converter={StaticResource StateToVisibilityConverter},
///             ConverterParameter={x:Static models:AppState.SetBeacon}}"
/// </summary>
public class StateToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Converts an AppState value to a boolean for visibility control.
    /// </summary>
    /// <param name="value">The current AppState value</param>
    /// <param name="targetType">The type of the binding target property (unused)</param>
    /// <param name="parameter">The target AppState to compare against</param>
    /// <param name="culture">Culture info (unused)</param>
    /// <returns>
    /// true: When current AppState matches the target state parameter
    /// false: When states don't match or for invalid/null input
    /// </returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is AppState currentState && parameter is AppState targetState)
        {
            return currentState == targetState;
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