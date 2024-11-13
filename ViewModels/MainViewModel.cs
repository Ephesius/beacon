using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Beacon.Models;
using Beacon.Services;
using System.ComponentModel;
using Microsoft.Extensions.Logging;

namespace Beacon.ViewModels;

/// <summary>
/// Primary view model managing the Beacon app's core functionality and UI state.
/// Implements MVVM pattern with ObservableObject for property change notifications.
/// Handles use interaction, location tracking, and navigation state transitions.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IBeaconLocationService _locationService;
    private readonly IStateService _stateService;
    private readonly ILogger<MainViewModel> _logger;

    /// <summary>
    /// Current application state controlling UI visibility and available actions.
    /// Changes trigger UI updates through StateToVisibilityConverter.
    /// </summary>
    [ObservableProperty]
    private AppState currentState;

    /// <summary>
    /// User-facing status message displayed in the bottom banner.
    /// Shows highest priortiy status from GPS signal, location updates, or general state. 
    /// </summary>
    [ObservableProperty]
    private string statusMessage = string.Empty;

    /// <summary>
    /// Current GPS signal status affecting UI indicators and featured availability.
    /// Controls GPS indicator color through SignalStatusToColorConverter.
    /// </summary>
    [ObservableProperty]
    private GPSStatus signalStatus;

    /// <summary>
    /// Distance to stored Beacon in meters.
    /// Updated during navigation, triggers destination state when reached.
    /// </summary>
    [ObservableProperty]
    private double distance;

    /// <summary>
    /// Current device compass orientation in degrees (0-359°).
    /// Used for navigation arrow rotation calculations.
    /// </summary>
    [ObservableProperty]
    private double deviceOrientation;

    /// <summary>
    /// Compass bearing to Beacon Location in degrees (0-359°).
    /// Combined with device orientation to determine arrow rotation.
    /// </summary>
    [ObservableProperty]
    private double bearingToBeacon;

    /// <summary>
    /// Final rotation angle for navigation arrow in degrees (0-359°).
    /// Calculated as (bearingToBeacon - deviceOrientation + 360) % 360.
    /// </summary>
    [ObservableProperty]
    private double arrowRotation;

    /// <summary>
    /// Currently stored Beacon Location.
    /// Null when no Beacon is <see langword="set"/>, controls app state transitions.
    /// </summary>
    [ObservableProperty]
    private BeaconLocation? storedBeacon;

    /// <summary>
    /// Initializes view model with required services and starts location tracking.
    /// Requests location permission if needed and restores previous app state.
    /// </summary>
    /// <param name="locationService">Service handling GPS and cmpass updates</param>
    /// <param name="stateService">Service managing Beacon and state persistence</param>
    /// <param name="logger">Logging service for error tracking</param>
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

    /// <summary>
    /// Sets new Beacon at current location if GPS signal is strong.
    /// Transitions to FindBeacon state on sucess.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Starts navigation to stored Beacon Location.
    /// Transitions to Navigation state and begins direction/distance updates.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Stops active navigation while preserving Beacon Location.
    /// Returns to FindBeacon state for later navigation resumption.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Completes current journey and clears stored Beacon.
    /// Transitions back to SetBeacon state for new journey.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Clears stored Beacon after user confirmation.
    /// Transitions to SetBeacon state if confirmed.
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task ClearBeacon()
    {
        bool shouldClear = await Shell.Current.DisplayAlert(
            "Clear Beacon",
            "Clear beacon location?",
            "Clear",
            "Cancel");

        if (shouldClear)
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

    /// <summary>
    /// Updates navigation information including distance and bearing to Beacon.
    /// Transitions to Destination state when Beacon is reached.
    /// </summary>
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

    /// <summary>
    /// Updates navigation arrow rotation based on current bearing and device orientation.
    /// Called when either bearing or orientation chagnes.
    /// </summary>
    private void UpdateArrowRotation()
    {
        ArrowRotation = (BearingToBeacon - DeviceOrientation + 360) % 360;
        _logger.LogInformation($"Arrow Update - Bearing: {BearingToBeacon}, Device: {DeviceOrientation}, Final Rotation: {ArrowRotation}");
    }
}