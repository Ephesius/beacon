# MainPage View Specification

## Required Visual Elements

Header Section:
| Element           | Type      | Constraints                  | States                          |
|------------------|-----------|------------------------------|----------------------------------|
| Title            | Text      | "Beacon"                     | Static                          |
| Subtitle         | Text      | "Never get lost again"       | Static                          |
| GPS Indicator    | Compound  | 44x44pt minimum              | Strong/Weak/None                |

GPS Status Indicator:
| State    | Color  | Icon       | Text         | Priority |
|----------|--------|------------|--------------|----------|
| Strong   | Green  | Checkmark  | Strong       | 1        |
| Weak     | Yellow | Warning    | Weak         | 1        |
| None     | Red    | X          | No Signal    | 1        |

Battery Status Indicator:
| Battery Level | Update Rate | Indicator Style              | Priority |
|--------------|-------------|------------------------------|----------|
| > 75%        | 1 second    | None                        | -        |
| 25-75%       | 3 seconds   | Small battery icon          | 3        |
| < 25%        | 5 seconds   | Red battery icon + text     | 2        |

Additional Status Indicators:
| Indicator          | Type           | Conditions                               | Priority |
|-------------------|----------------|------------------------------------------|----------|
| Battery Saving    | Banner         | Update interval > 1 second               | 3        |
| GPS-Only Mode     | Banner         | Compass unavailable                      | 2        |
| Calibrating       | Overlay        | During compass calibration               | 1        |
| Background Active | Icon           | App in background, tracking              | 2        |

Main Action Button:
| App State    | Text           | Icon          | Enabled When                |
|--------------|----------------|---------------|----------------------------|
| SetBeacon    | "Set Beacon"   | Anchor        | GPS Strong                 |
| FindBeacon   | "Find Beacon"  | Navigation    | Beacon stored              |
| Navigation   | "Stop"         | Stop          | Always                     |
| Destination  | "Complete"     | Checkmark     | Always                     |

Status Banner:
| Position     | Type           | Behavior                                  |
|--------------|----------------|-------------------------------------------|
| Bottom       | Banner         | Single message, highest priority shown    |
| Height       | Dynamic        | Based on content                          |
| Width        | Full          | Matches screen width                      |

Navigation Display:
| Element          | Type           | Format                    | Updates        |
|------------------|----------------|---------------------------|----------------|
| Direction Arrow  | SVG            | 0-359° rotation           | Real-time      |
| Distance         | Text           | "X meters"                | 1-5 seconds    |
| Compass Marks    | SVG            | N/S/E/W indicators        | Real-time      |

## Error Recovery UI
| Error State        | Primary UI                    | Secondary UI                  | Duration    |
|-------------------|-------------------------------|------------------------------|-------------|
| Signal Loss       | Red GPS indicator             | "Attempting to reconnect..." | Until fixed |
| Poor Accuracy     | Yellow GPS indicator          | "Finding better signal..."   | Until fixed |
| No Permission     | Permission dialog             | Settings redirect           | Until fixed |
| Compass Error     | Calibration overlay           | GPS-only mode indicator     | As needed   |

## Calibration System

Calibration States:
| State              | Trigger                      | UI Response                    |
|-------------------|------------------------------|--------------------------------|
| Not Required      | Normal operation             | No indication                  |
| Recommended       | Accuracy > 15°               | Small indicator                |
| Required          | Accuracy > 45°               | Full calibration overlay      |
| In Progress       | User calibrating             | Progress overlay              |
| Failed            | Timeout/Error                | Error + GPS fallback          |

Calibration Overlay:
| Element           | Type           | Content                                  |
|-------------------|----------------|------------------------------------------|
| Background        | Semi-transparent| 50% black                               |
| Title            | Text           | "Calibrate Compass"                      |
| Instructions     | Text           | "Wave device in figure-8 pattern"        |
| Animation        | SVG            | Figure-8 motion demonstration            |
| Progress         | Circular       | Shows calibration completion             |
| Cancel Button    | Button         | "Skip - Use GPS Only"                    |

Platform-Specific Calibration:
| Platform    | Calibration Method           | User Instruction                |
|------------|-----------------------------|---------------------------------|
| iOS        | System calibration overlay   | Show system UI                  |
| Android    | Custom calibration UI       | Show custom overlay             |

## Visual States

SetBeacon State:
| Element           | Visibility | Position        | Interaction               |
|-------------------|------------|-----------------|---------------------------|
| Header            | Visible    | Top             | None                     |
| GPS Indicator     | Visible    | Top Right       | None                     |
| Action Button     | Visible    | Center          | Tap to set beacon        |
| Status Banner     | Visible    | Bottom          | None                     |
| Navigation Display| Hidden     | -               | -                        |

FindBeacon State:
| Element           | Visibility | Position        | Interaction               |
|-------------------|------------|-----------------|---------------------------|
| Header            | Visible    | Top             | None                     |
| GPS Indicator     | Visible    | Top Right       | None                     |
| Action Button     | Visible    | Center          | Tap to start navigation  |
| Status Banner     | Visible    | Bottom          | None                     |
| Clear Option      | Visible    | Banner Right    | Tap to clear beacon      |
| Navigation Display| Hidden     | -               | -                        |

Navigation State:
| Element           | Visibility | Position        | Interaction               |
|-------------------|------------|-----------------|---------------------------|
| Header            | Visible    | Top             | None                     |
| GPS Indicator     | Visible    | Top Right       | None                     |
| Navigation Display| Visible    | Center          | None                     |
| Action Button     | Visible    | Bottom          | Tap to stop navigation   |
| Status Banner     | Visible    | Bottom          | None                     |

Destination State:
| Element           | Visibility | Position        | Interaction               |
|-------------------|------------|-----------------|---------------------------|
| Header            | Visible    | Top             | None                     |
| Message           | Visible    | Center          | None                     |
| Action Button     | Visible    | Bottom          | Tap to complete journey  |
| Status Banner     | Visible    | Bottom          | None                     |

Background State:
| Element           | Visibility | Behavior                                |
|-------------------|------------|----------------------------------------|
| Notification      | Visible    | Shows distance + direction             |
| Location Updates  | Active     | Per battery optimization rules         |
| Return Banner     | Visible    | "Tap to return to Beacon"             |

## Required Dialogs

Confirmation Dialogs:
| Action              | Message                                  | Buttons                |
|--------------------|-----------------------------------------|------------------------|
| Clear Beacon       | "Clear beacon location?"                 | Clear/Cancel           |
| Complete Journey   | "Complete journey and clear beacon?"     | Complete/Cancel        |
| Stop Navigation    | "Stop navigation? Beacon will remain set"| Stop/Cancel           |

Permission Dialogs:
| Type               | Message                                  | Buttons                |
|--------------------|-----------------------------------------|------------------------|
| Initial Request    | "Location needed to set/find beacon"     | Allow/Deny            |
| Settings Redirect  | "Please enable location in settings"     | Settings/Cancel       |
| Background         | "Allow background location updates?"     | Allow/Deny            |

## Layout Requirements
- Portrait orientation only
- Support system font scaling
- Minimum touch target: 44x44pt
- Contrast ratio: 4.5:1 minimum
- Support dark/light themes
- Safe area compliance
- Dynamic status bar color

## Event Handlers
| Event                 | Source Service          | UI Response                            | Priority |
|----------------------|------------------------|----------------------------------------|-----------|
| LocationChanged      | BeaconLocationService  | Update distance/direction             | 2         |
| SignalStatusChanged  | BeaconLocationService  | Update GPS indicator/status           | 1         |
| PermissionChanged    | BeaconLocationService  | Show appropriate dialog               | 1         |
| CalibrationNeeded    | BeaconLocationService  | Show calibration overlay             | 1         |
| StateChanged         | StateService          | Update UI state/elements              | 3         |
| BearingChanged      | BeaconLocationService  | Update direction arrow               | 2         |
| BatteryModeChanged   | BeaconLocationService  | Show/hide battery saving indicator   | 3         |
| FallbackModeChanged  | BeaconLocationService  | Show/hide GPS-only indicator         | 2         |
| BackgroundChanged    | BeaconLocationService  | Update notification/indicators       | 2         |

## Performance Requirements
| Action              | Maximum Latency | Visual Feedback                |
|--------------------|-----------------|-------------------------------|
| Button Press       | 100ms           | Immediate state change        |
| Location Update    | 1-5 seconds     | Smooth distance updates       |
| Direction Update   | 100ms           | Smooth arrow rotation         |
| Status Change      | 100ms           | Immediate indicator update    |
| Background Update  | 5 seconds       | Notification refresh          |
