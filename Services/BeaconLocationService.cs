using Beacon.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Devices.Sensors;
using IGeolocation = Microsoft.Maui.Devices.Sensors.IGeolocation;
using ICompass = Microsoft.Maui.Devices.Sensors.ICompass;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Microsoft.Extensions.Logging;

namespace Beacon.Services;

public partial class BeaconLocationService : ObservableObject, IBeaconLocationService
{
    private readonly IGeolocation _geolocation;
    private readonly ICompass _compass;
    private readonly ILogger<BeaconLocationService> _logger;
    private bool isTracking;

    [ObservableProperty]
    private BeaconLocation? currentLocation;

    [ObservableProperty]
    private double currentAccuracy;

    [ObservableProperty]
    private GPSStatus signalStatus;

    [ObservableProperty]
    private string statusMessage;

    [ObservableProperty]
    private PermissionStatus currentPermission;

    [ObservableProperty]
    private double deviceOrientation;

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

    private void Compass_ReadingChanged(object? sender, CompassChangedEventArgs e)
    {
        DeviceOrientation = e.Reading.HeadingMagneticNorth;
        _logger.LogInformation($"Device orientation updated: {DeviceOrientation}");
    }

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

    public async Task<PermissionStatus> CheckPermission()
    {
        return await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
    }

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

    public Task StopLocationUpdates()
    {
        isTracking = false;
        return Task.CompletedTask;
    }

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

    private void UpdateLocationFrequency()
    {
        if (isTracking)
        {
            StopLocationUpdates();
            _ = StartLocationUpdates();
        }
    }

    private void UpdateAccuracyTarget()
    {
        if (isTracking)
        {
            StopLocationUpdates();
            _ = StartLocationUpdates();
        }
    }

    private async void UpdateBackgroundMode()
    {
        if (isTracking)
        {
            await StopLocationUpdates();
            await StartLocationUpdates();
        }
    }
}