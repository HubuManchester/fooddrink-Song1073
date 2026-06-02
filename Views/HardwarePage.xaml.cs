using FoodDrinkApp.Services;

namespace FoodDrinkApp.Views;

public partial class HardwarePage : ContentPage
{
    private int feedbackTestCount = 0;

    public HardwarePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
    }

    /// <summary>
    /// Tests the camera hardware by capturing a photo with flashlight enabled.
    /// The flash is turned on before capture and off after to verify both camera and flashlight work.
    /// </summary>
    private async void OnTakePhotoClicked(object? sender, EventArgs e)
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                await DisplayAlert("Hardware Test", "Camera is not supported on this device.", "OK");
                return;
            }

            // Turn on flash before capture
            try { await Flashlight.Default.TurnOnAsync(); } catch { }

            FileResult? photo = await MediaPicker.Default.CapturePhotoAsync();

            // Turn off flash after capture
            try { await Flashlight.Default.TurnOffAsync(); } catch { }

            if (photo != null)
            {
                var stream = await photo.OpenReadAsync();
                HardwareImageView.Source = ImageSource.FromStream(() => stream);
            }
        }
        catch (Exception ex)
        {
            // Ensure flash is off even on error
            try { await Flashlight.Default.TurnOffAsync(); } catch { }
            await DisplayAlert("Camera Test Failed", $"Could not open camera: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// Tests GPS and geocoding hardware by fetching the current device location
    /// and performing a reverse geocode lookup.
    /// </summary>
    private async void OnGetLocationClicked(object? sender, EventArgs e)
    {
        try
        {
            CoordinateLabel.Text = "Fetching location...";
            LocationLabel.Text = "";
            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var location = await Geolocation.Default.GetLocationAsync(request);

            if (location != null)
            {
                CoordinateLabel.Text = $"Lat: {location.Latitude:F5}, Long: {location.Longitude:F5}";
                var placemarks = await Geocoding.Default.GetPlacemarksAsync(location.Latitude, location.Longitude);
                var placemark = placemarks?.FirstOrDefault();

                if (placemark != null)
                    LocationLabel.Text = $"{placemark.CountryName} / {placemark.AdminArea} / {placemark.Locality}";
            }
            else
            {
                CoordinateLabel.Text = "GPS returned no data.";
            }
        }
        catch (Exception ex)
        {
            CoordinateLabel.Text = "GPS test failed.";
            LocationLabel.Text = ex.Message;
        }
    }

    /// <summary>
    /// Tests the haptic / vibration motor hardware.
    /// </summary>
    private void OnHapticClicked(object? sender, EventArgs e)
    {
        try { HapticFeedback.Default.Perform(HapticFeedbackType.Click); } catch { }
        feedbackTestCount++;
        FeedbackCountLabel.Text = $"Haptic tests: {feedbackTestCount}";
    }
}