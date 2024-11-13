using Beacon.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;

namespace Beacon.Services;

/// <summary>
/// Defines the contract for Beacon Location services, handling GPS tracking,
/// compass orientation, and signal status management.
/// Implements INotifyPropertyChanged for UI updates.
/// </summary>
public interface IBeaconLocationService : INotifyPropertyChanged
{
    // Location Methods

    /// <summary>
    /// Requests Location permission from the user.
    /// Must be called before any Location operations.
    /// </summary>
    /// <returns>True if permission granted, false if denied</returns>
    Task<bool> RequestPermission();
    
    /// <summary>
    /// Checks current Location permission status.
    /// </summary>
    /// <returns>Current permission status from device settings</returns>
    Task<PermissionStatus> CheckPermission();

    /// <summary>
    /// Gets a single Location fix with maximum accuracy.
    /// Will only return Location if accuracy meets app requirements.
    /// </summary>
    /// <returns>Validated Beacon Location</returns>
    Task<BeaconLocation> GetCurrentLocation();

    /// <summary>
    /// Starts continuous location tracking with battery-aware updates.
    /// Update frequency varies with battery level (1-5 seconds).
    /// </summary>
    /// <returns></returns>
    Task StartLocationUpdates();

    /// <summary>
    /// Stops continuous location tracking.
    /// Location and signal status remain at last known values.
    /// </summary>
    /// <returns></returns>
    Task StopLocationUpdates();

    /// <summary>
    /// Attempts to recover from GPS signal loss.
    /// </summary>
    /// <returns>True if recovery attempt initiated</returns>
    Task<bool> HandleSignalLoss();

    // Observable Properties

    /// <summary>
    /// Current validated device location.
    /// </summary>
    BeaconLocation CurrentLocation { get; }

    /// <summary>
    /// Current GPS accuracy in meters.
    /// </summary>
    double CurrentAccuracy { get; }

    /// <summary>
    /// Current GPS signal status (None/Weak/Strong).
    /// Controls UI indicators and feature availability
    /// </summary>
    GPSStatus SignalStatus { get; }

    /// <summary>
    /// User-facing status message.
    /// </summary>
    string StatusMessage { get; }

    /// <summary>
    /// Current Location permission status.
    /// </summary>
    PermissionStatus CurrentPermission { get; }

    /// <summary>
    /// Current device compass orientation (0-359°).
    /// </summary>
    double DeviceOrientation { get; }

    // Configuration Properties

    /// <summary>
    /// Location update frequency (1-5 seconds)
    /// Adjusted based on battery level when BatteryAware is true.
    /// </summary>
    TimeSpan UpdateInterval { get; set; }

    /// <summary>
    /// Target GPS accuracy in meters
    /// </summary>
    double DesiredAccuracy { get; set; }

    /// <summary>
    /// Controls whether updates continue in background
    /// </summary>
    bool BackgroundEnabled { get; set; }

    /// <summary>
    /// Controls battery-aware update frequency adjustments.
    /// When enabled, updates slow down at lower battery levels.
    /// </summary>
    bool BatteryAware { get; set; }
}