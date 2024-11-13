using Beacon.Models;
using System.ComponentModel;

namespace Beacon.Services;

/// <summary>
/// Defines the contract for application state persistence and restoration.
/// Manages Beacon Location storage and application state transitions.
/// Implements INotifyPropertyChanged for UI updates.
/// 
/// Storage requirements:
/// - Beacon coordinates stored in secure device memory
/// - State survives app closure/restart
/// - Data cleared on journey completion or app uninstall
/// </summary>
public interface IStateService : INotifyPropertyChanged
{
    // State Methods

    /// <summary>
    /// Stores Beacon Location in secure device storage.
    /// Must encrypt sensitive location data.
    /// </summary>
    /// <param name="location">Validated Beacon Location to store</param>
    /// <returns>True if storage successful, false if failed</returns>
    Task<bool> StoreBeacon(BeaconLocation location);

    /// <summary>
    /// Retrieves stored Beacon Location if exists.
    /// </summary>
    /// <returns>Stored Beacon Location or null if none exists</returns>
    Task<BeaconLocation?> RetrieveBeacon();

    /// <summary>
    /// Removes stored Beacon Location from device storage.
    /// Called during journey completion or manual clear.
    /// </summary>
    /// <returns>True if clear successful, false if failed</returns>
    Task<bool> ClearBeacon();

    /// <summary>
    /// Retrieves last known application state.
    /// Used to restpre app state after closure/restart.
    /// </summary>
    /// <returns></returns>
    Task<AppState> GetLastKnownState();

    /// <summary>
    /// Persists current application state.
    /// Called during state transitions to ensure state survival.
    /// </summary>
    /// <param name="state">Current AppState to persist</param>
    /// <returns>True if save successful, false if failed</returns>
    Task<bool> SaveState(AppState state);

    // Events

    /// <summary>
    /// Triggered when stored Beacon Location changes.
    /// Provides null when Beacon is cleared.
    /// </summary>
    event EventHandler<BeaconLocation?> BeaconChanged;

    /// <summary>
    /// Triggered when application state changes.
    /// Used to update UI and trigger state-specific behaviors.
    /// </summary>
    event EventHandler<AppState> StateChanged;
}
