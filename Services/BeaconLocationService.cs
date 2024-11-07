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
    private double currentBearing;

    [ObservableProperty]
    private string? calibrationStatus;

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

    private bool isTracking;

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
            var location = await _geolocation.GetLocationAsync(new GeolocationRequest
            {
                DesiredAccuracy = GeolocationAccuracy.Best,
                Timeout = TimeSpan.FromSeconds(5)
            });

            if (location != null)
            {
                return new BeaconLocation(
                    location.Latitude,
                    location.Longitude,
                    location.Accuracy ?? double.MaxValue);
            }

            throw new Exception("Could not get location"); // Or we could create a custom LocationException
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get location");
            StatusMessage = "Unable to get location";
            throw; // Re-throw to let caller handle the failure
        }
    }

    public async Task StartLocationUpdates()
    {
        if (isTracking) return;

        try
        {
            isTracking = true;
            while (isTracking)
            {
                var location = await _geolocation.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Best,
                    Timeout = TimeSpan.FromSeconds(5)
                });

                if (location != null)
                {
                    CurrentLocation = new BeaconLocation(
                        location.Latitude,
                        location.Longitude,
                        location.Accuracy ?? double.MaxValue);

                    CurrentAccuracy = location.Accuracy ?? double.MaxValue;
                    UpdateSignalStatus(location.Accuracy ?? double.MaxValue);
                }

                await Task.Delay(UpdateInterval);
                await OptimizeForBattery();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start location updates");
            StatusMessage = "Failed to start location tracking";
        }
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

    public Task<bool> StartCompass()
    {
        if (!HasMagnetometer()) return Task.FromResult(false);

        try
        {
            _compass.ReadingChanged += Compass_ReadingChanged;
            _compass.Start(SensorSpeed.UI);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start compass");
            return Task.FromResult(false);
        }
    }

    private void Compass_ReadingChanged(object? sender, CompassChangedEventArgs e)
    {
        CurrentBearing = e.Reading.HeadingMagneticNorth;
    }

    public Task StopCompass()
    {
        _compass.ReadingChanged -= Compass_ReadingChanged;
        _compass.Stop();
        return Task.CompletedTask;
    }

    public Task<bool> CalibrateCompass()
    {
        CalibrationStatus = "Calibrating compass...";
        return Task.FromResult(true);
    }

    public bool HasMagnetometer() => _compass.IsSupported;

    public async Task<bool> FallbackToGPSOnly()
    {
        await StopCompass();
        StatusMessage = "Using GPS only mode";
        return true;
    }

    private void UpdateSignalStatus(double accuracy)
    {
        SignalStatus = accuracy switch
        {
            <= 5.0 => GPSStatus.Strong,
            <= 10.0 => GPSStatus.Weak,
            _ => GPSStatus.None
        };

        StatusMessage = SignalStatus switch
        {
            GPSStatus.Strong => "GPS signal strong",
            GPSStatus.Weak => "GPS signal weak",
            _ => "No GPS signal"
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