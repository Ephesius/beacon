# Services

## IBeaconLocationService
Primary service for location, compass, and sensor management.

Location Methods:
| Method               | Parameters   | Returns            | Purpose                                   |
|---------------------|--------------|--------------------|--------------------------------------------|
| RequestPermission   | None         | Task\<bool>        | Request location permission from user      |
| CheckPermission     | None         | PermissionStatus   | Get current permission state              |
| GetCurrentLocation  | None         | Task\<BeaconLocation> | Get single location fix                |
| StartLocationUpdates| None         | None               | Begin continuous location monitoring      |
| StopLocationUpdates | None         | None               | Stop continuous location monitoring       |
| HandleSignalLoss    | None         | Task\<bool>        | Attempt signal recovery                   |

Location Events:
| Event               | Arguments                    | Priority | Triggers When                               |
|--------------------|------------------------------|----------|---------------------------------------------|
| LocationChanged    | BeaconLocation               | 2        | New location received (1-5 sec intervals)   |
| AccuracyChanged    | double                       | 1        | GPS accuracy changes                        |
| SignalStatusChanged| GPSStatus, StatusMessage     | 1        | Signal strength category changes            |
| PermissionChanged  | PermissionStatus, StatusMessage| 1      | Location permission state changes           |

Compass Methods:
| Method              | Parameters   | Returns         | Purpose                                    |
|--------------------|--------------|-----------------|---------------------------------------------|
| StartCompass       | None         | Task\<bool>     | Initialize compass/begin bearing updates    |
| StopCompass        | None         | None           | Stop compass updates                        |
| CalibrateCompass   | None         | Task\<bool>     | Trigger system compass calibration         |
| HasMagnetometer    | None         | bool           | Check if device has compass hardware        |
| FallbackToGPSOnly  | None         | Task\<bool>     | Switch to GPS-only mode                    |

Compass Events:
| Event              | Arguments          | Priority | Triggers When                                |
|-------------------|-------------------|-----------|---------------------------------------------|
| BearingChanged    | double            | 2         | Device bearing changes (0-359°)             |
| CalibrationNeeded | StatusMessage     | 1         | System requests compass calibration          |

Configuration Properties:
| Property           | Type        | Default    | Range          | Purpose                                    |
|-------------------|-------------|------------|----------------|---------------------------------------------|
| UpdateInterval    | TimeSpan    | 1 second   | 1-5 seconds    | Location update frequency                   |
| DesiredAccuracy   | double      | 5.0        | ≤ 5.0 meters   | Target GPS accuracy                        |
| BackgroundEnabled | bool        | false      | -              | Allow background location updates          |
| BatteryAware     | bool        | true       | -              | Adjust updates based on battery level      |

Battery Optimization:
| Battery Level | Update Interval | Accuracy Target |
|--------------|-----------------|-----------------|
| > 75%        | 1 second        | 5.0 meters      |
| 25-75%       | 3 seconds       | 5.0 meters      |
| < 25%        | 5 seconds       | 5.0 meters      |

Error Recovery Actions:
| Error State    | Primary Action              | Secondary Action          | User Message Required |
|---------------|----------------------------|-------------------------|---------------------|
| Signal Loss   | HandleSignalLoss()         | Increase update interval | Yes                 |
| Poor Accuracy | Wait for better signal     | Increase update interval | Yes                 |
| No Permission | RequestPermission()        | Direct to settings      | Yes                 |
| No Compass    | FallbackToGPSOnly()       | None                    | Yes                 |

## IStateService
Manages application state persistence and restoration.

State Methods:
| Method                 | Parameters       | Returns            | Purpose                                |
|-----------------------|------------------|--------------------|-----------------------------------------|
| StoreBeacon           | BeaconLocation   | Task\<bool>        | Save beacon location                   |
| RetrieveBeacon        | None             | Task\<BeaconLocation?> | Get stored beacon if exists        |
| ClearBeacon           | None             | Task\<bool>        | Remove stored beacon                   |
| GetLastKnownState     | None             | Task\<AppState>    | Retrieve last app state               |
| SaveState             | AppState         | Task\<bool>        | Persist current app state             |

Events:
| Event                | Arguments    | Triggers When                              |
|---------------------|--------------|-------------------------------------------|
| BeaconChanged       | BeaconLocation? | Stored beacon location changes          |
| StateChanged        | AppState     | Application state changes                  |

Security Requirements:
| Requirement         | Implementation Detail                                    |
|--------------------|---------------------------------------------------------|
| Storage Location   | Device secure preferences                                |
| Data Encryption    | Platform-provided encryption                             |
| Data Lifetime      | Cleared on journey completion or app uninstall           |
| Access Control     | Private to app, no external access                       |
