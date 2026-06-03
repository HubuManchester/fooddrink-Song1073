using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodDrinkApp.Services;

namespace FoodDrinkApp.ViewModels;

public partial class HardwareViewModel : BaseViewModel
{
    private int _feedbackTestCount = 0;

    [ObservableProperty]
    private ImageSource? _hardwareImageSource;

    [ObservableProperty]
    private string _coordinateText = "Coordinates will appear here.";

    [ObservableProperty]
    private string _locationText = "Address will appear here.";

    [ObservableProperty]
    private string _feedbackCountText = "Haptic tests: 0";

    [RelayCommand]
    private async Task TakePhotoAsync()
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                if (Shell.Current?.CurrentPage != null)
                    await Shell.Current.CurrentPage.DisplayAlert("Hardware Test", "Camera is not supported on this device.", "OK");
                return;
            }

            try { await Flashlight.Default.TurnOnAsync(); } catch { }

            FileResult? photo = await MediaPicker.Default.CapturePhotoAsync();

            try { await Flashlight.Default.TurnOffAsync(); } catch { }

            if (photo != null)
            {
                var stream = await photo.OpenReadAsync();
                HardwareImageSource = ImageSource.FromStream(() => stream);
            }
        }
        catch (Exception ex)
        {
            try { await Flashlight.Default.TurnOffAsync(); } catch { }
            if (Shell.Current?.CurrentPage != null)
                await Shell.Current.CurrentPage.DisplayAlert("Camera Test Failed", $"Could not open camera: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task GetLocationAsync()
    {
        try
        {
            CoordinateText = "Fetching location...";
            LocationText = "";
            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var location = await Geolocation.Default.GetLocationAsync(request);

            if (location != null)
            {
                CoordinateText = $"Lat: {location.Latitude:F5}, Long: {location.Longitude:F5}";
                var placemarks = await Geocoding.Default.GetPlacemarksAsync(location.Latitude, location.Longitude);
                var placemark = placemarks?.FirstOrDefault();

                if (placemark != null)
                    LocationText = $"{placemark.CountryName} / {placemark.AdminArea} / {placemark.Locality}";
            }
            else
            {
                CoordinateText = "GPS returned no data.";
            }
        }
        catch (Exception ex)
        {
            CoordinateText = "GPS test failed.";
            LocationText = ex.Message;
        }
    }

    [RelayCommand]
    private void Haptic()
    {
        try { HapticFeedback.Default.Perform(HapticFeedbackType.Click); } catch { }
        _feedbackTestCount++;
        FeedbackCountText = $"Haptic tests: {_feedbackTestCount}";
    }
}
