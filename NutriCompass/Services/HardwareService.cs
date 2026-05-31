using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;

namespace NutriCompass.Services;

public sealed class HardwareService : IHardwareService
{
    private readonly object _orientationLock = new();
    private readonly SensorSpeed _sensorSpeed = SensorSpeed.UI;
    private HardwareOrientation _lastOrientation = HardwareOrientation.Unknown;

    public event EventHandler<HardwareOrientationChangedEventArgs>? OrientationChanged;

    public void StartOrientationUpdates()
    {
        lock (_orientationLock)
        {
            if (Accelerometer.IsMonitoring)
            {
                return;
            }

            Accelerometer.ReadingChanged += OnAccelerometerReadingChanged;
            Accelerometer.Start(_sensorSpeed);
        }
    }

    public void StopOrientationUpdates()
    {
        lock (_orientationLock)
        {
            if (!Accelerometer.IsMonitoring)
            {
                return;
            }

            Accelerometer.ReadingChanged -= OnAccelerometerReadingChanged;
            Accelerometer.Stop();
        }
    }

    public async Task<HardwareLocationResult> GetLocationAsync(CancellationToken cancellationToken)
    {
        var request = new GeolocationRequest(GeolocationAccuracy.Best);
        var location = await Geolocation.GetLocationAsync(request, cancellationToken);
        if (location == null)
        {
            throw new InvalidOperationException("Geolocation returned null.");
        }

        return new HardwareLocationResult(location.Latitude, location.Longitude, location.Accuracy ?? 0);
    }

    public void Dispose()
    {
        StopOrientationUpdates();
    }

    private void OnAccelerometerReadingChanged(object? sender, AccelerometerChangedEventArgs e)
    {
        var orientation = MapToOrientation(e.Reading);
        if (orientation == _lastOrientation)
        {
            return;
        }

        _lastOrientation = orientation;
        OrientationChanged?.Invoke(this, new HardwareOrientationChangedEventArgs(orientation));
    }

    private static HardwareOrientation MapToOrientation(AccelerometerData data)
    {
        var absX = Math.Abs(data.Acceleration.X);
        var absY = Math.Abs(data.Acceleration.Y);
        return absX > absY ? HardwareOrientation.Landscape : HardwareOrientation.Portrait;
    }
}
