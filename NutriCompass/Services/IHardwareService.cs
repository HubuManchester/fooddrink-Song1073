using System;
using System.Threading;
using System.Threading.Tasks;

namespace NutriCompass.Services;

public enum HardwareOrientation
{
    Unknown,
    Portrait,
    Landscape
}

public sealed class HardwareOrientationChangedEventArgs : EventArgs
{
    public HardwareOrientationChangedEventArgs(HardwareOrientation orientation) => Orientation = orientation;
    public HardwareOrientation Orientation { get; }
}

public sealed class HardwareLocationResult
{
    public HardwareLocationResult(double latitude, double longitude, double accuracy)
    {
        Latitude = latitude;
        Longitude = longitude;
        Accuracy = accuracy;
    }

    public double Latitude { get; }
    public double Longitude { get; }
    public double Accuracy { get; }
}

public interface IHardwareService : IDisposable
{
    event EventHandler<HardwareOrientationChangedEventArgs>? OrientationChanged;

    void StartOrientationUpdates();
    void StopOrientationUpdates();
    Task<HardwareLocationResult> GetLocationAsync(CancellationToken cancellationToken);
}
