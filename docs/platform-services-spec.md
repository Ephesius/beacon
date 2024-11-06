# Platform Services Specification

## Location Services

Android Implementation:
| Service              | Framework           | Minimum Requirements               | Permissions                     |
|---------------------|---------------------|-----------------------------------|--------------------------------|
| Location Provider   | Google Play Services| API Level 26 (Android 8.0)       | ACCESS_FINE_LOCATION           |
| Background Location | WorkManager         | 15-minute minimum interval        | ACCESS_BACKGROUND_LOCATION     |
| Power Management    | Doze Mode Whitelist | Battery optimization exemption    | REQUEST_IGNORE_BATTERY_OPTIMIZATIONS |

iOS Implementation:
| Service              | Framework           | Minimum Requirements               | Authorization                   |
|---------------------|---------------------|-----------------------------------|--------------------------------|
| Location Provider   | CoreLocation        | iOS 14+                          | When In Use                    |
| Background Location | CLLocationManager   | Always authorization             | Always                         |
| Power Management    | Background Modes    | Location updates capability      | Background Modes Entitlement   |

## Compass/Sensors

Android Implementation:
| Sensor              | Type                | Update Frequency                  | Calibration                     |
|---------------------|---------------------|-----------------------------------|--------------------------------|
| Compass             | TYPE_MAGNETIC_FIELD | 1-5 Hz based on battery          | System calibration dialog       |
| Accelerometer       | TYPE_ACCELEROMETER  | 1-5 Hz based on battery          | Not required                    |
| Orientation         | Combined sensors    | 1-5 Hz based on battery          | Follows compass calibration     |

iOS Implementation:
| Sensor              | Framework           | Update Frequency                  | Calibration                     |
|---------------------|---------------------|-----------------------------------|--------------------------------|
| Compass             | CoreLocation        | 1-5 Hz based on battery          | System calibration overlay      |
| Heading             | CLHeading           | 1-5 Hz based on battery          | System calibration overlay      |
| Motion              | CoreMotion          | 1-5 Hz based on battery          | Not required                    |

## Background Operation

Android Task Configuration:
| Component           | Type                | Constraints                       | Battery Impact                  |
|---------------------|---------------------|-----------------------------------|--------------------------------|
| WorkManager Job     | Periodic            | 15-minute minimum interval        | Minimal - System optimized      |
| Foreground Service  | Location Service    | Notification required            | Medium - User visible           |
| Background Limits   | Android 8.0+        | Limited to foreground service    | Based on system settings        |

iOS Background Modes:
| Mode                | Capability          | Constraints                       | Battery Impact                  |
|---------------------|---------------------|-----------------------------------|--------------------------------|
| Location Updates    | Always authorized   | Continuous updates allowed        | Medium - System optimized       |
| Background Task     | BGTaskScheduler     | System determined intervals       | Minimal - System managed        |
| Background Fetch    | Limited updates     | System determined frequency       | Minimal - System managed        |

## State Persistence

Android Storage:
| Data Type           | Storage Method      | Security Level                    | Lifecycle                       |
|---------------------|---------------------|-----------------------------------|--------------------------------|
| Beacon Location     | EncryptedSharedPrefs| Encrypted at rest                | Cleared on journey completion   |
| App State           | EncryptedSharedPrefs| Encrypted at rest                | Survives app restart            |
| Calibration Data    | SharedPreferences   | Private to app                   | Temporary - session only        |

iOS Storage:
| Data Type           | Storage Method      | Security Level                    | Lifecycle                       |
|---------------------|---------------------|-----------------------------------|--------------------------------|
| Beacon Location     | Keychain           | Encrypted at rest                 | Cleared on journey completion   |
| App State           | UserDefaults       | Private to app                    | Survives app restart            |
| Calibration Data    | In-Memory          | Temporary only                    | Temporary - session only        |

## UI Requirements

Android Components:
| Element             | Implementation      | Theme Requirements               | Accessibility                   |
|---------------------|---------------------|----------------------------------|--------------------------------|
| Status Bar          | System.StatusBar    | Dynamic color based on theme     | Support contrast modes          |
| Notifications      | NotificationCompat  | Platform standard style          | Support screen readers          |
| Dialogs            | MaterialAlertDialog | Material Design 3                | Standard focus navigation       |
| Permissions        | ActivityCompat      | System standard                  | Standard permissions flow       |

iOS Components:
| Element             | Implementation      | Theme Requirements               | Accessibility                   |
|---------------------|---------------------|----------------------------------|--------------------------------|
| Status Bar          | UIStatusBar        | Adapts to light/dark mode        | Support contrast modes          |
| Notifications      | UNUserNotification | System standard style            | Support VoiceOver              |
| Dialogs            | UIAlertController  | System standard                  | Standard focus navigation       |
| Permissions        | CLLocationManager  | System standard                  | Standard permissions flow       |

## Power Management

Android Optimizations:
| Battery Level  | Location Frequency | Compass Updates | Background              |
|---------------|-------------------|-----------------|------------------------|
| > 75%         | 1 second          | 1 Hz            | Normal priority        |
| 25-75%        | 3 seconds         | 0.5 Hz          | Lower priority         |
| < 25%         | 5 seconds         | 0.2 Hz          | Minimum priority       |

iOS Optimizations:
| Battery Level  | Location Frequency | Compass Updates | Background              |
|---------------|-------------------|-----------------|------------------------|
| > 75%         | 1 second          | 1 Hz            | Normal priority        |
| 25-75%        | 3 seconds         | 0.5 Hz          | Reduced accuracy       |
| < 25%         | 5 seconds         | 0.2 Hz          | Significant changes    |

## Error Handling

Android Platform Errors:
| Error Type          | Detection Method   | Recovery Action                   | User Message Required         |
|---------------------|-------------------|-----------------------------------|------------------------------|
| Google Play Missing | Service check     | Direct to Play Store              | Yes                          |
| Location Disabled   | System settings   | Open location settings            | Yes                          |
| Permission Denied   | Runtime check     | Show rationale & request again    | Yes                          |
| Sensors Missing     | Feature check     | Switch to GPS-only mode           | Yes                          |

iOS Platform Errors:
| Error Type          | Detection Method   | Recovery Action                   | User Message Required         |
|---------------------|-------------------|-----------------------------------|------------------------------|
| Authorization Denied| Status check      | Direct to Settings app            | Yes                          |
| Location Disabled   | System settings   | Direct to Settings app            | Yes                          |
| Accuracy Restricted | Accuracy check    | Request temporary full accuracy   | Yes                          |
| Heading Unavailable | Sensor check      | Switch to GPS-only mode           | Yes                          |

## Performance Standards

Android:
| Operation           | Maximum Latency   | Battery Impact                    | Memory Usage                   |
|---------------------|-------------------|-----------------------------------|--------------------------------|
| Location Update     | 1-5 seconds       | < 5% per hour                     | < 50MB active                  |
| Compass Update      | 100ms             | < 2% per hour                     | < 10MB active                  |
| Background Task     | 15 minutes        | < 1% per hour                     | < 20MB background              |

iOS:
| Operation           | Maximum Latency   | Battery Impact                    | Memory Usage                   |
|---------------------|-------------------|-----------------------------------|--------------------------------|
| Location Update     | 1-5 seconds       | < 5% per hour                     | < 50MB active                  |
| Compass Update      | 100ms             | < 2% per hour                     | < 10MB active                  |
| Background Task     | System managed    | < 1% per hour                     | < 20MB background              |
