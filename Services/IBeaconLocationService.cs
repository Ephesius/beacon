using Beacon.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;

namespace Beacon.Services;

public interface IBeaconLocationService : INotifyPropertyChanged
{
    // Location Methods
    Task<bool> RequestPermission();
    Task<PermissionStatus> CheckPermission();
    Task<BeaconLocation> GetCurrentLocation();
    Task StartLocationUpdates();
    Task StopLocationUpdates();
    Task<bool> HandleSignalLoss();

    // Observable Properties
    BeaconLocation CurrentLocation { get; }
    double CurrentAccuracy { get; }
    GPSStatus SignalStatus { get; }
    string StatusMessage { get; }
    PermissionStatus CurrentPermission { get; }
    double DeviceOrientation { get; }

    // Configuration Properties
    TimeSpan UpdateInterval { get; set; }
    double DesiredAccuracy { get; set; }
    bool BackgroundEnabled { get; set; }
    bool BatteryAware { get; set; }
}