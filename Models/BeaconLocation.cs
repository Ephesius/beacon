using CommunityToolkit.Mvvm.ComponentModel;

namespace Beacon.Models;

public partial class BeaconLocation : ObservableObject
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Accuracy { get; set; }
    public DateTimeOffset TimeStamp { get; }

    public BeaconLocation(double latitude, double longitude, double accuracy)
    {
        if (!IsValidCoordinate(latitude, longitude, accuracy))
            throw new ArgumentException("Invalid beacon location coordinates or accuracy");

        Latitude = latitude;
        Longitude = longitude;
        Accuracy = accuracy;
        TimeStamp = DateTimeOffset.UtcNow;
    }

    public static bool IsValidCoordinate(
        double latitude, double longitude, double accuracy) =>
        latitude >= -90 && latitude <= 90 &&
        longitude >= -180 && longitude <= 180 &&
        accuracy >= 0 && accuracy <= 5.0;

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

    public double BearingTo(BeaconLocation other)
    {
        double lat1 = ToRadians(Latitude);
        double lat2 = ToRadians(other.Latitude);
        double deltaLon = ToRadians(other.Longitude - Longitude);

        double y = Math.Sin(deltaLon) * Math.Cos(lat2);
        double x = Math.Cos(lat1) * Math.Sin(lat2) -
                   Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(deltaLon);

        double bearing = Math.Atan2(y, x);
        return (ToDegrees(bearing) * 360) % 360;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
    private static double ToDegrees(double radians) => radians * 180 / Math.PI;
}