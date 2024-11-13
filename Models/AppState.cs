namespace Beacon.Models;

/// <summary>
/// Defines the primary states of the Beacon application.
/// This enum drives the app's UI state machine and determines visible components.
/// </summary>
public enum AppState
{
    /// <summary>
    /// Initial state when no beacon is set.
    /// UI shows: "Set Beacon" button, GPS status indicator
    /// Requirements: Strong GPS signal to proceed
    /// </summary>
    SetBeacon,

    /// <summary>
    /// State after beacon is set but navigation hasn't started.
    /// UI shows: "Find Beacon" button, clear beacon option
    /// Transitions to: Navigation (on button press) or SetBeaon (on clear)
    /// </summary>
    FindBeacon,

    /// <summary>
    /// Active navigation state with live updates.
    /// UI shows: Direction arrow, distance-to-go display, "Stop Navigation" button
    /// Transitions to: Destination (within specified range) or FindBeacon (on stop)
    /// </summary>
    Navigation,

    /// <summary>
    /// Final state when user reaches beacon location within specified range.
    /// UI shows: Success message, "Complete Journey" button
    /// Transitions to: SetBeacon (on journey completion)
    /// </summary>
    Destination
}
