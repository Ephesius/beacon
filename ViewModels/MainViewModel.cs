using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Beacon.Models;
using Beacon.Services;
using Microsoft.Maui;
using System.Net.NetworkInformation;

namespace Beacon.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IBeaconLocationService _locationService;
    private readonly IStateService _stateService;

    [ObservableProperty]
    private AppState _currentState;

    [ObservableProperty]
    private BeaconLocation? _storedBeacon;

    [ObservableProperty]
    private BeaconLocation? _currentLocation;

    [ObservableProperty]
    private double _bearing;

    [ObservableProperty]
    private double _distance;

    [ObservableProperty]
    private GPSStatus _gpsStatus = GPSStatus.None;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public MainViewModel(IBeaconLocationService locationService, IStateService stateService)
    {
        _locationService = locationService;
        _stateService = stateService;

        // Subscribe to service events
        _locationService.LocationChanged += OnLocationChanged;
        _locationService.SignalStatusChanged += OnSignalStatusChanged;
        _locationService.BearingChanged += OnBearingChanged;
        _stateService.BeaconChanged += OnBeaconChanged;

        InitializeAsync().FireAndForgetSafeAsync();
    }

    private async Task InitializeAsync()
    {
        _currentState = await _stateService.GetLastKnownState();
        _storedBeacon = await _stateService.RetrieveBeacon();
        await _locationService.StartLocationUpdates();
    }

    [RelayCommand(CanExecute = nameof(CanSetBeacon))]
    private async Task SetBeaconAsync()
    {
        if (_currentLocation == null) return;

        await _stateService.StoreBeacon(_currentLocation);
        await _stateService.SaveState(AppState.FindBeacon);
    }

    [RelayCommand]
    private async Task StartNavigationAsync()
    {
        await _locationService.StartCompass();
        await _stateService.SaveState(AppState.Navigation);
    }

    [RelayCommand]
    private async Task StopNavigationAsync()
    {
        await _locationService.StopCompass();
        await _stateService.SaveState(AppState.FindBeacon);
    }

    [RelayCommand]
    private async Task CompleteJourneyAsync()
    {
        await _stateService.ClearBeacon();
        await _locationService.StopCompass();
        await _stateService.SaveState(AppState.SetBeacon);
    }
    private bool CanSetBeacon =>
        _gpsStatus == GPSStatus.Strong && _currentLocation?.Accuracy <= 5.0;

    private void OnLocationChanged(BeaconLocation location)
    {
        CurrentLocation = location;
        if (StoredBeacon != null)
        {
            Distance = location.DistanceTo(StoredBeacon);
            if (Distance <= 5.0 && CurrentState == AppState.Navigation)
            {
                _stateService.SaveState(AppState.Destination)
                    .FireAndForgetSafeAsync();
            }
        }
    }

    private void OnSignalStatusChanged(GPSStatus status, string message)
    {
        GpsStatus = status;
        StatusMessage = message;
    }

    private void OnBearingChanged(double newBearing)
    {
        Bearing = newBearing;
    }

    private void OnBeaconChanged(BeaconLocation? beacon)
    {
        StoredBeacon = beacon;
    }
}