# Models

## BeaconLocation
An immutable value type representing a geographical beacon point.

Properties:
| Name      | Type            | Constraints                    | Purpose                           |
|-----------|-----------------|-------------------------------|-----------------------------------|
| Latitude  | Double         | Range: -90 to 90              | Geographical latitude position     |
| Longitude | Double         | Range: -180 to 180            | Geographical longitude position    |
| Accuracy  | Double         | Maximum: 5.0 meters           | GPS accuracy at time of capture    |
| Timestamp | DateTimeOffset | UTC with offset               | When beacon was set               |

Required Operations:
| Operation          | Parameters                    | Returns    | Purpose                               |
|-------------------|------------------------------|------------|---------------------------------------|
| Equality Check    | Other BeaconLocation         | Boolean    | Value-based location comparison       |
| Distance To       | Other BeaconLocation         | Double     | Distance in meters between points     |
| Bearing To        | Other BeaconLocation         | Double     | Compass bearing (0-359°) to target    |
| Validation        | Lat, Long, Accuracy          | Boolean    | Validates coordinates and accuracy     |

## LocationError
Structured error information for location-related failures.

Types:
| Error Type           | Priority | Conditions                      | Recovery Action                  |
|---------------------|----------|--------------------------------|----------------------------------|
| InvalidCoordinates  | 1        | Out of range lat/long           | Require new coordinate capture   |
| InsufficientAccuracy| 1        | Accuracy > 5.0m                 | Wait for better GPS signal       |
| SignalLost          | 1        | No GPS fix                      | Show reconnection instructions   |
| PermissionDenied    | 1        | Location permission not granted  | Direct to settings              |

Required Error Data:
| Field     | Type     | Purpose                                  |
|-----------|----------|------------------------------------------|
| Type      | Enum     | Categorizes the error for handling       |
| Priority  | Int      | 1-3 matching design's status hierarchy   |
| Message   | String   | User-presentable error description       |
| Technical | String   | Detailed information for logging         |
| Timestamp | DateTime | When the error occurred                  |
