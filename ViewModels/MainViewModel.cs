using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Beacon.Models;
using Beacon.Services;
using System.ComponentModel;
using Microsoft.Extensions.Logging;

namespace Beacon.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IBeaconLocationService _locationService;
    private readonly IStateService _stateService;
    private readonly ILogger<MainViewModel> _logger;

    [ObservableProperty]
    private AppState currentState;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private GPSStatus signalStatus;

    [ObservableProperty]
    private double distance;

    [ObservableProperty]
    private double deviceOrientation;

    [ObservableProperty]
    private double bearingToBeacon;

    [ObservableProperty]
    private double arrowRotation;

    [ObservableProperty]
    private BeaconLocation? storedBeacon;

    public MainViewModel(
        IBeaconLocationService locationService,
        IStateService stateService,
        ILogger<MainViewModel> logger)
    {
        _locationService = locationService;
        _stateService = stateService;
        _logger = logger;

        _locationService.PropertyChanged += LocationService_PropertyChanged;
        _stateService.BeaconChanged += StateService_BeaconChanged;
        _stateService.StateChanged += StateService_StateChanged;

        _ = Initialize();
    }

    private async Task Initialize()
    {
        try
        {
            var permissionStatus = await _locationService.CheckPermission();
            if (permissionStatus != PermissionStatus.Granted)
            {
                var granted = await _locationService.RequestPermission();
                if (!granted)
                {
                    StatusMessage = "Location permission required";
                    return;
                }
            }

            await _locationService.StartLocationUpdates();

            StoredBeacon = await _stateService.RetrieveBeacon();
            CurrentState = StoredBeacon != null ? AppState.FindBeacon : AppState.SetBeacon;
            await _stateService.SaveState(CurrentState);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize MainViewModel");
            StatusMessage = "Failed to initialize app";
        }
    }

    [RelayCommand]
    private async Task SetBeacon()
    {
        try
        {
            var location = await _locationService.GetCurrentLocation();
            if (location != null)
            {
                await _stateService.StoreBeacon(location);
                CurrentState = AppState.FindBeacon;
                await _stateService.SaveState(CurrentState);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set beacon");
            StatusMessage = "Failed to set beacon location";
        }
    }

    [RelayCommand]
    private async Task StartNavigation()
    {
        if (StoredBeacon == null) return;

        try
        {
            CurrentState = AppState.Navigation;
            await _stateService.SaveState(CurrentState);
            UpdateNavigationInfo();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start navigation");
            StatusMessage = "Failed to start navigation";
        }
    }

    [RelayCommand]
    private async Task StopNavigation()
    {
        try
        {
            CurrentState = AppState.FindBeacon;
            await _stateService.SaveState(CurrentState);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop navigation");
            StatusMessage = "Failed to stop navigation";
        }
    }

    [RelayCommand]
    private async Task CompleteJourney()
    {
        try
        {
            await _stateService.ClearBeacon();
            CurrentState = AppState.SetBeacon;
            await _stateService.SaveState(CurrentState);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete journey");
            StatusMessage = "Failed to complete journey";
        }
    }

    [RelayCommand]
    private async Task ClearBeacon()
    {
        try
        {
            await _stateService.ClearBeacon();
            CurrentState = AppState.SetBeacon;
            await _stateService.SaveState(CurrentState);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear beacon");
            StatusMessage = "Failed to clear beacon";
        }
    }

    private void LocationService_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IBeaconLocationService.CurrentLocation):
                UpdateNavigationInfo();
                break;
            case nameof(IBeaconLocationService.DeviceOrientation):
                DeviceOrientation = _locationService.DeviceOrientation;
                if (StoredBeacon != null && _locationService.CurrentLocation != null)
                {
                    // Need to make sure we have BearingToBeacon before updating rotation
                    BearingToBeacon = _locationService.CurrentLocation.BearingTo(StoredBeacon);
                    UpdateArrowRotation();
                }
                break;
            case nameof(IBeaconLocationService.SignalStatus):
                SignalStatus = _locationService.SignalStatus;
                break;
            case nameof(IBeaconLocationService.StatusMessage):
                StatusMessage = _locationService.StatusMessage;
                break;
        }
    }

    private void StateService_BeaconChanged(object? sender, BeaconLocation? location)
    {
        StoredBeacon = location;
        if (location == null && CurrentState != AppState.SetBeacon)
        {
            CurrentState = AppState.SetBeacon;
            _ = _stateService.SaveState(CurrentState);
        }
    }

    private void StateService_StateChanged(object? sender, AppState state)
    {
        CurrentState = state;
    }

    private void UpdateNavigationInfo()
    {
        if (StoredBeacon != null && _locationService.CurrentLocation != null)
        {
            Distance = _locationService.CurrentLocation.DistanceTo(StoredBeacon);
            BearingToBeacon = _locationService.CurrentLocation.BearingTo(StoredBeacon);
            UpdateArrowRotation();  // This is already correct - it's called after we have BearingToBeacon

            if (Distance <= 5.0 && CurrentState == AppState.Navigation)
            {
                CurrentState = AppState.Destination;
                _ = _stateService.SaveState(CurrentState);
            }
        }
    }

    private void UpdateArrowRotation()
    {
        ArrowRotation = (BearingToBeacon - DeviceOrientation + 360) % 360;
        _logger.LogInformation($"Arrow Update - Bearing: {BearingToBeacon}, Device: {DeviceOrientation}, Final Rotation: {ArrowRotation}");
    }
}