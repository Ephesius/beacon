using Beacon.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Beacon.Services;

/// <summary>
/// Manages application state persistence and restoration, handling secure storage of Beacon Locations
/// and application state transitions. Implements IStateService for dependency injection.
/// 
/// Storage Requirements:
/// - Beacon coordinates stored in SecureStorage with encryption
/// - App State persisted in Preferences
/// - All data automatically cleared on journey completion or app uninstall
/// </summary>
public partial class StateService : ObservableObject, IStateService
{
    private readonly ILogger<StateService> _logger;
    private const string BeaconKey = "stored_beacon";
    private const string StateKey = "app_state";

    /// <summary>
    /// Event triggered when application state changes.
    /// Provides null when Beacon is cleared.
    /// </summary>
    public event EventHandler<BeaconLocation?>? BeaconChanged;
    
    /// <summary>
    /// Event triggered when application state changes.
    /// Used to update UI and trigger state-specific behaviors.
    /// </summary>
    public event EventHandler<AppState>? StateChanged;

    /// <summary>
    /// Initializes the state service with required dependencies.
    /// </summary>
    /// <param name="logger">Logging service for error tracking and debugging</param>
    public StateService(ILogger<StateService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Stores a Beacon Location in SecureStorage with encryption.
    /// Triggers BeaconChanged event on successful storage.
    /// </summary>
    /// <param name="location">Validated Beacon Locaiton to store</param>
    /// <returns>True if storage successful, false if failed</returns>
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

    /// <summary>
    /// Retrieves stored Beacon Location from SecureStorage fi exists.
    /// Returns null if no Beacon is stored <see langword="or"/>if retrieval fails.
    /// </summary>
    /// <returns>Stored Beacon Location or null</returns>
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

    /// <summary>
    /// Removes stored Beacon Location from SecureStorage.
    /// Triggers BeaconChanged event with null value or sucessful removal.
    /// </summary>
    /// <returns><see langword="true"/>if clear successful, false if failed</returns>
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

    /// <summary>
    /// Retrieves last known application state from Preferences.
    /// Returns SetBeacon state if no stored state exists.
    /// </summary>
    /// <returns>Last known AppState or SetBeacon default</returns>
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

    /// <summary>
    /// Persists current application state in Preferences.
    /// Triggers StateChanged event on successful save.
    /// </summary>
    /// <param name="state"></param>
    /// <returns>True if save successful, false if failed</returns>
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
