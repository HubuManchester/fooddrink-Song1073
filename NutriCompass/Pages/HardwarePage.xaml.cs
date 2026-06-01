using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Haptics;
using Microsoft.Maui.Devices.Sensors;

namespace NutriCompass.Pages;

public partial class HardwarePage : AccessibleContentPage
{
    private int _hapticCount;
    private CancellationTokenSource? _locationCancellationToken;

    public HardwarePage()
    {
        InitializeComponent();
    }

    private async void OnRefreshLocationClicked(object? sender, EventArgs e)
    {
        await RefreshLocationAsync();
    }

    private async Task RefreshLocationAsync()
    {
        UpdateLocationLabels("Checking location permission...", "Requesting...", "Requesting...");

        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            if (status != PermissionStatus.Granted)
            {
                UpdateLocationLabels("Location permission denied.", "N/A", "N/A");
                return;
            }

            LocationStatusLabel.Text = "Acquiring location...";
            ResetLocationCancellationToken();

            var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
            var location = await Geolocation.Default.GetLocationAsync(request, _locationCancellationToken.Token);

            if (location is null)
            {
                UpdateLocationLabels("Unable to determine location.", "N/A", "N/A");
                return;
            }

            CoordinatesValueLabel.Text = $"{location.Latitude:F4}, {location.Longitude:F4}";
            LocationStatusLabel.Text = "Location acquired.";
            await ResolvePlacemarkAsync(location);
        }
        catch (FeatureNotSupportedException)
        {
            UpdateLocationLabels("Location services not supported.", "N/A", "N/A");
        }
        catch (PermissionException)
        {
            UpdateLocationLabels("Location permission denied.", "N/A", "N/A");
        }
        catch (Exception)
        {
            UpdateLocationLabels("Unable to load location.", "N/A", "N/A");
        }
    }

    private async Task ResolvePlacemarkAsync(Location location)
    {
        try
        {
            var token = _locationCancellationToken?.Token ?? CancellationToken.None;
            var placemarks = await Geocoding.Default.GetPlacemarksAsync(location.Latitude, location.Longitude, token);
            var placemark = placemarks?.FirstOrDefault();
            if (placemark is null)
            {
                UpdatePlacemark("No placemark data.");
                return;
            }

            var components = new List<string>();
            void AddIfNotEmpty(string? value)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    components.Add(value);
                }
            }

            AddIfNotEmpty(placemark.CountryName);
            AddIfNotEmpty(placemark.AdminArea);
            AddIfNotEmpty(placemark.Locality);
            AddIfNotEmpty(placemark.SubLocality);
            if (components.Count == 0)
            {
                AddIfNotEmpty(placemark.FeatureName);
            }

            UpdatePlacemark(components.Count > 0 ? string.Join(", ", components) : "No placemark data.");
        }
        catch (Exception)
        {
            UpdatePlacemark("Unable to reverse geocode.");
        }
    }

    private void OnHapticButtonClicked(object? sender, EventArgs e)
    {
        _hapticCount++;
        try
        {
            HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
        }
        catch
        {
            // Device may not support haptic feedback; ignore.
        }

        HapticCountLabel.Text = $"Haptic taps: {_hapticCount}";
    }

    private void UpdateLocationLabels(string status, string coordinates, string placemark)
    {
        LocationStatusLabel.Text = status;
        CoordinatesValueLabel.Text = coordinates;
        PlacemarkValueLabel.Text = placemark;
    }

    private void UpdatePlacemark(string value)
    {
        PlacemarkValueLabel.Text = value;
    }

    private void ResetLocationCancellationToken()
    {
        _locationCancellationToken?.Cancel();
        _locationCancellationToken?.Dispose();
        _locationCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(20));
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _locationCancellationToken?.Cancel();
        _locationCancellationToken?.Dispose();
        _locationCancellationToken = null;
    }
}
