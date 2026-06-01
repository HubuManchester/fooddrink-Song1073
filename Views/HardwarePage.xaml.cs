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

    private async void OnTakePhotoClicked(object? sender, EventArgs e)
    {
        try
        {
            if (MediaPicker.Default.IsCaptureSupported)
            {
                FileResult? photo = await MediaPicker.Default.CapturePhotoAsync();
                if (photo != null)
                {
                    var stream = await photo.OpenReadAsync();
                    HardwareImageView.Source = ImageSource.FromStream(() => stream);
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"Could not open camera: {ex.Message}", "OK");
        }
    }

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
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"Could not get location: {ex.Message}", "OK");
        }
    }

    private void OnHapticClicked(object? sender, EventArgs e)
    {
        try { HapticFeedback.Default.Perform(HapticFeedbackType.Click); } catch { }
        feedbackTestCount++;
        FeedbackCountLabel.Text = $"Haptic tests: {feedbackTestCount}";
    }
}