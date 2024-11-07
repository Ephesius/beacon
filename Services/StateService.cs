using Beacon.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Beacon.Services;

public partial class StateService : ObservableObject, IStateService
{
    private readonly ILogger<StateService> _logger;
    private const string BeaconKey = "stored_beacon";
    private const string StateKey = "app_state";

    public event EventHandler<BeaconLocation?>? BeaconChanged;
    public event EventHandler<AppState>? StateChanged;

    public StateService(ILogger<StateService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> StoreBeacon(BeaconLocation location)
    {
        try
        {
            var json = JsonSerializer.Serialize(location);
            await SecureStorage.SetAsync(BeaconKey, json);
            BeaconChanged?.Invoke(this, location);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store beacon location");
            return false;
        }
    }

    public async Task<BeaconLocation?> RetrieveBeacon()
    {
        try
        {
            var json = await SecureStorage.GetAsync(BeaconKey);
            if (string.IsNullOrEmpty(json))
                return null;

            return JsonSerializer.Deserialize<BeaconLocation>(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve beacon location");
            return null;
        }
    }

    public Task<bool> ClearBeacon()
    {
        try
        {
            SecureStorage.Remove(BeaconKey);
            BeaconChanged?.Invoke(this, null);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear beacon location");
            return Task.FromResult(false);
        }
    }

    public Task<AppState> GetLastKnownState()
    {
        try
        {
            var stateString = Preferences.Get(StateKey, AppState.SetBeacon.ToString());
            return Task.FromResult(Enum.Parse<AppState>(stateString));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get las known state");
            return Task.FromResult(AppState.SetBeacon);
        }
    }

    public Task<bool> SaveState(AppState state)
    {
        try
        {
            Preferences.Set(StateKey, state.ToString());
            StateChanged?.Invoke(this, state);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save state");
            return Task.FromResult(false);
        }
    }
}
