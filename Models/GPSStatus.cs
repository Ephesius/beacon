namespace Beacon.Models;

/// <summary>
/// Defines GPS signal status categories for Location tracking.
/// Used to control UI indicator and app behavior based on signal quality.
/// Visual indicators:
/// - None (Red): No usable signal
/// - Weak (Yellow): Poor accuracy
/// -Strong (Green): Good accuracy
/// </summary>
public enum GPSStatus
{
    /// <summary>
    /// No usable GPS signal available.
    /// UI: Red indicator
    /// Action: Location-dependent features disabled
    /// </summary>
    None,

    /// <summary>
    /// Poor GPS signal with reduced accuracy.
    /// UI: Yellow indicator
    /// Action: Warning displayed, waiting for better signal
    /// </summary>
    Weak,
    
    /// <summary>
    /// Strong GPS signal with good accuracy
    /// UI: Green indicator
    /// Action: All location features enabled
    /// </summary>
    Strong  // Good signal
}
