# Beacon App Design Document

## Overview
Beacon is a minimalist navigation app designed with a single purpose: helping users find their way back to a marked location. Whether parking a car, starting a hike, or setting a meeting point at a festival, Beacon makes it impossible to get lost with just two taps.

## Core Principles
- Extreme simplicity
- Minimal user interaction required
- Clear, focused functionality
- Persistent state (survives app closure/restart)
- No unnecessary features
- Works offline by default
- Respects battery life
- Platform-native look and feel
- Clear status communication at all times

## Technical Requirements
- Location permissions required
- Background location access optional
- Device sensors required: GPS and magnetometer
- Minimum iOS 14 / Android 8.0
- Internet connection optional
- Persistent storage for beacon data
- Portrait orientation lock

## Thresholds & Limits
- Arrival detection threshold: 5 meters
- Target GPS accuracy: 5 meters or better
- Minimum sensor update frequency: 1 second
- Maximum sensor update frequency: 5 seconds when moving

## UI/UX Standards
- Portrait orientation only (enforced)
- System font sizes respected
- Platform theme (light/dark) adherence
- Distance format: "X meters" (e.g., "47 meters")
-Status banner shows all app states:
  - Calculating position
  - Signal strength changes
  - Navigation updates
  - error conditions
- GPS status indicator:
  - Green dot: Strong signal
  - Yellow dot: Weak signal
  - Red dot: No signal
- Standard platform alerts for notifications/confirmations
- No custom transitions/animations
- Immediate state transitions (no animations)
- Status banner shows single highest-priority status:
  1. GPS signal issues (highest priority)
  2. Navigation status
  3. General app state (lowest priority)
- Claer, focused message only
- No multiple status messages

## Error States & Recovery
- GPS Signal Loss:
  - Display status via indicator
  - Show alert on complete signal loss
  - Auto-resume when signal returns
- Permission Denial:
  - Initial denial: Offer immediate retry
  - Permanent denial: Direct to system settings with explanation
  - Runtime revocation: Alert user and prompt to restore
- Sensor Issues:
  - Magnetometer unavailable: Notify user of GPS-only mode
  - Calibration needed: Handle system calibration prompts gracefully

## User Flow
1. First launch → Request location permission
2. Open app → See "Set Beacon" screen
3. Tap to set beacon → See "Find Beacon" screen with status
4. Later, tap "Find Beacon" → See navigation screen
5. Reach destination → See completion screen
6. Complete journey → Return to initial screen

## Main Page States

### State 1: Set Beacon
- Header: "Beacon" with "Never get lost again" subtitle
- Large circular button with anchor icon
- Text: "Set Beacon"
- Clean, minimal design
- GPS status indicator in corner

### State 2: Find Beacon
- Same header
- Large circular button with navigation icon
- Text: "Find Beacon"
- Bottom status banner: "Beacon Has Been Set" with subtle clear option (×)
- Clear confirmation dialog when requested
- GPS status indicator in corner

### State 3: Navigation
- Same header
- Large directional arrow with compass markers
- Distance to beacon display
- Bottom button: "Stop Navigation" with "(Beacon will still be set)"
- GPS status indicator in corner

### State 4: Destination
- Same header
- "You made it!" message
- "Complete Journey" button
- Confirmation dialog before clearing beacon

## Project Structure
```
Beacon/
├── Models/
│   └── BeaconLocation.cs
├── Services/
│   ├── IBeaconLocationService.cs
│   ├── IStateService.cs
│   ├── BeaconLocationService.cs
│   └── StateService.cs
├── ViewModels/
│   └── MainViewModel.cs
├── Views/
│   ├── MainPage.xaml
│   └── MainPage.xaml.cs
├── Platforms/
│   ├── Android/
│   ├── iOS/
│   └── [Other platform-specific folders]
├── Resources/
│   ├── AppIcon/
│   ├── Fonts/
│   ├── Images/
│   └── Styles/
├── App.xaml
├── AppShell.xaml
├── MauiProgram.cs
└── README.md
```

## Implementation Notes

### Location Services
- GPS coordinates captured when beacon is set (highest precision possible)
- Real-time location tracking during navigation
- Straight-line distance calculation
- Compass integration for direction
- Graceful fallback to GPS-only when magnetometer unavailable

### State Management
- Beacon coordinates persisted locally
- App state survives closure/restart
- Complete journey resets app to initial state (clearing all UI elements, stored locations, and state)
- Handles runtime permission changes

### UI Components
- Reusable button styles
- Status banner component
- Navigation compass/arrow
- Distance display
- Confirmation dialogs
- GPS status indicator

# Privacy & Data Handling
- No analytics or tracking
- Location data management:
  - Stored in secure device memory only
  - Never transmitted externally 
  - Automatically cleared:
    - Upon reaching beacon
    - When manually clearing beacon
    - During app uninstall
- Required privacy policy explaining:
  - Location permission usage
  - Local-only data storage
  - No data collection/sharing
  - No persistent storage
  - No third-party access

# Accessibility Standards
- High contrast visual elements throughout
- GPS status indicators combine:
  - Color: Green/Yellow/Red
  - Icon: Checkmark/Exclamation/X
  - Text: "Strong"/"Weak"/"No Signal"
- All interactive elements:
  - Minimum touch target size: 44x44pt
  - Clear visual boundaries
  - Distinct active/inactive states
- Text and scaling:
  - System font sizes respected
  - Support for dynamic type
  - Minimum contrast ratio 4.5:1
- Distance display:
  - Both visual and numeric representation
  - Clear, readable font size
  - High contrast against background
