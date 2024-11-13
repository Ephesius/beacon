namespace Beacon.Models;

/// <summary>
/// Represents an immutable geographical beacon point with validation and distance calculation capabilities.
/// All coordinates must be valid within standard GPS ranges and meet accuracy requirements.
/// </summary>
public record BeaconLocation
{
    /// <summary>
    /// The geographical Latitude in decimal degrees.
    /// Valid range: -90 to 90 degrees.
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// The geographical Longitude in decimal degrees.
    /// Valid range: -180 to 180 degrees.
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    /// GPS accuracy in meters at time of capture.
    /// Maximum allowed value based on specified accuracy requirements.
    /// </summary>
    public double Accuracy { get; set; }

    /// <summary>
    /// UTC timestamp when the beacon was set, stored with timezone offset.
    /// </summary>
    public DateTimeOffset TimeStamp { get; }

    /// <summary>
    /// Creates a new Beacon Location with validation.
    /// </summary>
    /// <param name="latitude">Latitude in decimal degrees (-90 to 90)</param>
    /// <param name="longitude">Longitude in decimal degrees (-180 to 180)</param>
    /// <param name="accuracy">GPS accuracy in meters</param>
    /// <exception cref="ArgumentException"></exception>
    public BeaconLocation(double latitude, double longitude, double accuracy)
    {
        if (!IsValidCoordinate(latitude, longitude, accuracy))
            throw new ArgumentException("Invalid beacon location coordinates or accuracy");

        Latitude = latitude;
        Longitude = longitude;
        Accuracy = accuracy;
        TimeStamp = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Validates coordinates and accuracy against app requirements.
    /// </summary>
    /// <param name="latitude">Latitude to validate (-90 to 90)</param>
    /// <param name="longitude">Longitude to validate (-180 to 180)</param>
    /// <param name="accuracy">Accuracy to validate</param>
    /// <returns>
    /// True if all values are within valid ranges, false otherwise
    /// </returns>
    public static bool IsValidCoordinate(
        double latitude, double longitude, double accuracy) =>
        latitude >= -90 && latitude <= 90 &&
        longitude >= -180 && longitude <= 180 &&
        accuracy >= 0 && accuracy <= 5.0;

    /// <summary>
    /// Calculates the great-circle distance to another Beacon Location.
    /// Uses the Haversine formula for accuracy over curved earth surface.
    /// </summary>
    /// <param name="other">Target Beacon Location</param>
    /// <returns>Distance in meters</returns>
    public double DistanceTo(BeaconLocation other)
    {
        const double earthRadius = 6371000; // meters
        double lat1 = ToRadians(Latitude);
        double lat2 = ToRadians(other.Latitude);
        double deltaLat = ToRadians(other.Latitude - Latitude);
        double deltaLon = ToRadians(other.Longitude - Longitude);

        double a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                   Math.Cos(lat1) * Math.Cos(lat2) *
                   Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadius * c;
    }

    /// <summary>
    /// Calculates the initial bearing (compass heading) to another Beacon Location.
    /// Used for navigation arrow orientation.
    /// </summary>
    /// <param name="other">Target Beacon Location</param>
    /// <returns>Bearing in degrees (0-35°)</returns>
    public double BearingTo(BeaconLocation other)
    {
        double lat1 = ToRadians(Latitude);
        double lat2 = ToRadians(other.Latitude);
        double deltaLon = ToRadians(other.Longitude - Longitude);

        double y = Math.Sin(deltaLon) * Math.Cos(lat2);
        double x = Math.Cos(lat1) * Math.Sin(lat2) -
                   Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(deltaLon);

        double bearing = Math.Atan2(y, x);
        double degrees = ToDegrees(bearing);
        return (degrees + 360) % 360;
    }

    /// <summary>
    /// Converts degrees to radians for mathematical calculations.
    /// </summary>
    /// <param name="degrees">Degree input</param>
    /// <returns>Radian output</returns>
    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
    
    /// <summary>
    /// Converts radians back to degrees for bearing results.
    /// </summary>
    /// <param name="radians">Radian input</param>
    /// <returns>Degree output</returns>
    private static double ToDegrees(double radians) => radians * 180 / Math.PI;
}