using Beacon.Models;
using System.ComponentModel;

namespace Beacon.Services;

public interface IStateService : INotifyPropertyChanged
{
    // State Methods
    Task<bool> StoreBeacon(BeaconLocation location);
    Task<BeaconLocation?> RetrieveBeacon();
    Task<bool> ClearBeacon();
    Task<AppState> GetLastKnownState();
    Task<bool> SaveState(AppState state);

    // Events
    event EventHandler<BeaconLocation?> BeaconChanged;
    event EventHandler<AppState> StateChanged;
}
