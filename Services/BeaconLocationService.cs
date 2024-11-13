using Beacon.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using ICompass = Microsoft.Maui.Devices.Sensors.ICompass;
using IGeolocation = Microsoft.Maui.Devices.Sensors.IGeolocation;

namespace Beacon.Services;

/// <summary>
/// Primary service handling location tracking, compass orientation, and GPS status management.
/// Inherits from ObservableObject to provide property change notifications.
/// Implements the IBeaconLocationService interface for dependency injection and service registration.
/// Executes battery-aware updates and accuracy-based signal status monitoring.
/// Update frequencies:
/// - Battery: 75%: 1 second
/// - Battery: 25-75%: 3 seconds
/// - Battery: Less than 25%: 5 seconds
/// </summary>
public partial class BeaconLocationService : ObservableObject, IBeaconLocationService
{
    private readonly IGeolocation _geolocation;
    private readonly ICompass _compass;
    private readonly ILogger<BeaconLocationService> _logger;
    private bool isTracking;

    /// <summary>
    /// Current drvice location with validated coordinates and accuracy.
    /// </summary>
    [ObservableProperty]
    private BeaconLocation? currentLocation;

    /// <summary>
    /// Current GPS accuracy in meters.
    /// </summary>
    [ObservableProperty]
    private double currentAccuracy;

    /// <summary>
    /// Current GPS signal status affecting UI indicators and app behavior.
    /// </summary>
    [ObservableProperty]
    private GPSStatus signalStatus;

    /// <summary>
    /// User-facing status message for current location service state.
    /// </summary>
    [ObservableProperty]
    private string statusMessage;

    /// <summary>
    /// Current location permission status from the device.
    /// </summary>
    [ObservableProperty]
    private PermissionStatus currentPermission;

    /// <summary>
    /// Current device compass orientation in degrees (0-359°).
    /// </summary>
    [ObservableProperty]
    private double deviceOrientation;

    /// <summary>
    /// Location update frequency, adjusted based on battery level.
    /// Range: 1-5 seconds
    /// </summary>
    private TimeSpan updateInterval = TimeSpan.FromSeconds(1);
    public TimeSpan UpdateInterval
    {
        get => updateInterval;
        set
        {
            if (SetProperty(ref updateInterval, value))
            {
                UpdateLocationFrequency();
            }
        }
    }

    /// <summary>
    /// Target GPS accuracy in meters.
    /// </summary>
    private double desiredAccuracy = 5.0;
    public double DesiredAccuracy
    {
        get => desiredAccuracy;
        set
        {
            if (SetProperty(ref desiredAccuracy, value))
            {
                UpdateAccuracyTarget();
            }
        }
    }

    /// <summary>
    /// Controls whether location updates continue in background.
    /// </summary>
    private bool backgroundEnabled;
    public bool BackgroundEnabled
    {
        get => backgroundEnabled;
        set
        {
            if (SetProperty(ref backgroundEnabled, value))
            {
                UpdateBackgroundMode();
            }
        }
    }

    /// <summary>
    /// Controls battery-aware update frequency adjustments
    /// When enabled, updates slow down at lower battery levels.
    /// </summary>
    private bool batteryAware = true;
    public bool BatteryAware
    {
        get => batteryAware;
        set
        {
            if (SetProperty(ref batteryAware, value))
            {
                _ = OptimizeForBattery();
            }
        }
    }

    /// <summary>
    /// Initializes the location service with required dependencies.
    /// Sets up compass tracking if available and initializes GPS monitoring.
    /// </summary>
    /// <param name="geolocation">Platform Location service</param>
    /// <param name="compass">Platform compass service</param>
    /// <param name="logger">Logging service</param>
    public BeaconLocationService(
        IGeolocation geolocation,
        ICompass compass,
        ILogger<BeaconLocationService> logger)
    {
        _geolocation = geolocation;
        _compass = compass;
        _logger = logger;

        SignalStatus = GPSStatus.None;
        StatusMessage = "Waiting for GPS signal...";

        // Start orientation tracking if available
        if (_compass.IsSupported)
        {
            _compass.ReadingChanged += Compass_ReadingChanged;
            try
            {
                _compass.Start(SensorSpeed.Game);
                _logger.LogInformation("Device orientation tracking started");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start orientation tracking");
            }
        }
    }

    /// <summary>
    /// Handles compass reading updates and updates device orientation.
    /// Called at frequency determined by SensorSpeed.Game setting.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Compass_ReadingChanged(object? sender, CompassChangedEventArgs e)
    {
        DeviceOrientation = e.Reading.HeadingMagneticNorth;
        _logger.LogInformation($"Device orientation updated: {DeviceOrientation}");
    }

    /// <summary>
    /// Requests location permission from the user.
    /// <see langword="required"/>for all location operations.
    /// </summary>
    /// <returns>True if permission granted, false otherwise</returns>
    public async Task<bool> RequestPermission()
    {
        try
        {
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            CurrentPermission = status;
            return status == PermissionStatus.Granted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to request location permission");
            return false;
        }
    }

    /// <summary>
    /// Checks current Location permission status on the device.
    /// </summary>
    /// <returns>Current permission status from the device</returns>
    public async Task<PermissionStatus> CheckPermission()
    {
        return await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
    }

    /// <summary>
    /// Gets a single location fix with maximum accuracy.
    /// Only return location if accuracy meets app requirements.
    /// </summary>
    /// <returns>Validated Beacon Location or throws if accuracy is insufficient</returns>
    /// <exception cref="Exception">Thrown when location unavailable or accuracy insufficient</exception>
    public async Task<BeaconLocation> GetCurrentLocation()
    {
        try
        {
            var request = new GeolocationRequest
            {
                DesiredAccuracy = GeolocationAccuracy.Best,
                Timeout = TimeSpan.FromSeconds(5)
            };

            var location = await _geolocation.GetLocationAsync(request)
                ?? throw new Exception("Could not get location");

            if (await _geolocation.GetLastKnownLocationAsync() is Location lastLocation)
            {
                // Use last known location if it's more accurate
                if (lastLocation.Accuracy < location.Accuracy)
                {
                    location = lastLocation;
                }
            }

            // Validate accuracy requirement
            var accuracy = location.Accuracy ?? double.MaxValue;
            if (accuracy > 5.0)
            {
                StatusMessage = "Waiting for better GPS accuracy...";
                throw new Exception("Insufficient GPS accuracy");
            }

            return new BeaconLocation(
                location.Latitude,
                location.Longitude,
                accuracy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get location");
            StatusMessage = "Unable to get location. Please check location services are enabled.";
            throw;
        }
    }

    /// <summary>
    /// Initiates continuous location tracking with batter-aware updates.
    /// Update frequency varies based on battery level:
    /// - High battery (75%+): 1 second
    /// - Medium battery (25-75%): 3 seconds
    /// - Low battery (0-25%): 5 seconds
    /// </summary>
    /// <returns></returns>
    public Task StartLocationUpdates()
    {
        if (isTracking) return Task.CompletedTask;

        isTracking = true;

        // Start the updates on a background task
        _ = Task.Run(async () =>
        {
            try
            {
                while (isTracking)
                {
                    try
                    {
                        var request = new GeolocationRequest
                        {
                            DesiredAccuracy = GeolocationAccuracy.Best,
                            Timeout = TimeSpan.FromSeconds(5)
                        };

                        var location = await _geolocation.GetLocationAsync(request);

                        if (location != null)
                        {
                            CurrentAccuracy = location.Accuracy ?? double.MaxValue;
                            UpdateSignalStatus(CurrentAccuracy);

                            // Only update location if it's valid
                            if (BeaconLocation.IsValidCoordinate(
                                location.Latitude,
                                location.Longitude,
                                CurrentAccuracy))
                            {
                                CurrentLocation = new BeaconLocation(
                                    location.Latitude,
                                    location.Longitude,
                                    CurrentAccuracy);
                            }
                        }
                        else
                        {
                            UpdateSignalStatus(double.MaxValue);
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        // Timeout is expected occasionally, just update status and continue
                        UpdateSignalStatus(double.MaxValue);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error during location update");
                        StatusMessage = "Location update error";

                        // Short pause before retrying
                        await Task.Delay(TimeSpan.FromSeconds(1));
                    }

                    await Task.Delay(UpdateInterval);
                    await OptimizeForBattery();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start location updates");
                StatusMessage = "Failed to start location tracking";
                isTracking = false;
            }
        });

        return Task.CompletedTask;
    }

    /// <summary>
    /// Stops continuous location tracking.
    /// Location and signal status will remain at last known values.
    /// </summary>
    /// <returns></returns>
    public Task StopLocationUpdates()
    {
        isTracking = false;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Attempts to recover from GPS signal loss by restarting location updates.
    /// </summary>
    /// <returns>True if recovery attemp initiated, false if recovery failed</returns>
    public async Task<bool> HandleSignalLoss()
    {
        try
        {
            await StopLocationUpdates();
            await Task.Delay(1000); // Brief pause before retry
            await StartLocationUpdates();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to recover from signal loss");
            return false;
        }
    }

    /// <summary>
    /// Updates GPS signal status and corresponding user message based on accuracy.
    /// </summary>
    /// <param name="accuracy"></param>
    private void UpdateSignalStatus(double accuracy)
    {
        SignalStatus = accuracy switch
        {
            <= 6.0 => GPSStatus.Strong,
            <= 12.0 => GPSStatus.Weak,
            _ => GPSStatus.None
        };

        StatusMessage = SignalStatus switch
        {
            GPSStatus.Strong => "GPS signal strong",
            GPSStatus.Weak => "GPS signal weak",
            _ => "Waiting for better GPS signal..."
        };
    }

    /// <summary>
    /// Adjusts update frequency based on current battery level.
    /// Implements the battery optimization strategy:
    /// - Battery 75%+: 1 second updates
    /// - Battery 25-75%: 3 second updates
    /// - Battery 0-25%: 5 second updates
    /// Only applies when BatteryAware is enabled.
    /// </summary>
    /// <returns></returns>
    private Task OptimizeForBattery()
    {
        if (!BatteryAware) return Task.CompletedTask;

        var level = Battery.Default.ChargeLevel;
        UpdateInterval = level switch
        {
            > 0.75 => TimeSpan.FromSeconds(1),
            > 0.25 => TimeSpan.FromSeconds(3),
            _ => TimeSpan.FromSeconds(5)
        };

        return Task.CompletedTask;
    }

    /// <summary>
    /// Updates location tracking frequency when UpdateInterval changes.
    /// Restarts tracking if current active to apply new interval.
    /// </summary>
    private void UpdateLocationFrequency()
    {
        if (isTracking)
        {
            StopLocationUpdates();
            _ = StartLocationUpdates();
        }
    }

    /// <summary>
    /// Updates the desired GPS accuracy target.
    /// Restarts tracking if currently active to apply new accuracy target.
    /// </summary>
    private void UpdateAccuracyTarget()
    {
        if (isTracking)
        {
            StopLocationUpdates();
            _ = StartLocationUpdates();
        }
    }

    /// <summary>
    /// Updates background tracking mode.
    /// Restarts location tracking to apply new background settings.
    /// </summary>
    private async void UpdateBackgroundMode()
    {
        if (isTracking)
        {
            await StopLocationUpdates();
            await StartLocationUpdates();
        }
    }
}