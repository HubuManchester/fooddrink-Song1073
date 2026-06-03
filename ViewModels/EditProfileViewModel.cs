using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodDrinkApp.Models;
using FoodDrinkApp.Services;

namespace FoodDrinkApp.ViewModels;

public partial class EditProfileViewModel : BaseViewModel
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _gender = string.Empty;

    [ObservableProperty]
    private string _age = string.Empty;

    [ObservableProperty]
    private string _height = string.Empty;

    [ObservableProperty]
    private string _weight = string.Empty;

    [ObservableProperty]
    private string _activityLevel = string.Empty;

    [ObservableProperty]
    private string _goal = string.Empty;

    [ObservableProperty]
    private string _locationText = "No location set.";

    [ObservableProperty]
    private string _profileImagePath = string.Empty;

    [ObservableProperty]
    private double _latitude;

    [ObservableProperty]
    private double _longitude;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoImage))]
    private bool _hasImage;

    public bool HasNoImage => !HasImage;

    [ObservableProperty]
    private ImageSource? _imageSource;

    public List<string> Genders { get; } = new() { "Male", "Female" };
    public List<string> ActivityLevels { get; } = new() { "Sedentary", "Lightly Active", "Moderately Active", "Very Active", "Extra Active" };
    public List<string> Goals { get; } = new() { "Lose Weight", "Maintain", "Gain Weight" };

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task TakeProfilePhotoAsync()
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                if (Shell.Current?.CurrentPage != null)
                    await Shell.Current.CurrentPage.DisplayAlert("Not Supported", "Camera is not available on this device.", "OK");
                return;
            }

            var photo = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions { Title = "Take Profile Photo" });
            if (photo != null)
            {
                await ToggleFlashForCapture();
                await SaveAndShowProfileImage(photo);
            }
        }
        catch (Exception ex)
        {
            if (Shell.Current?.CurrentPage != null)
                await Shell.Current.CurrentPage.DisplayAlert("Error", $"Could not take photo: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task PickProfilePhotoAsync()
    {
        try
        {
            var photo = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions { Title = "Select Profile Photo" });
            if (photo != null)
            {
                await SaveAndShowProfileImage(photo);
            }
        }
        catch (Exception ex)
        {
            if (Shell.Current?.CurrentPage != null)
                await Shell.Current.CurrentPage.DisplayAlert("Error", $"Could not pick photo: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task RemoveProfilePhotoAsync()
    {
        if (string.IsNullOrEmpty(ProfileImagePath))
        {
            try { HapticFeedback.Default.Perform(HapticFeedbackType.LongPress); } catch { }
            await Task.Delay(50);
            if (Shell.Current?.CurrentPage != null)
                await Shell.Current.CurrentPage.DisplayAlert("Error", "No photo to remove.", "OK");
            return;
        }

        ProfileImagePath = string.Empty;
        ImageSource = null;
        HasImage = false;
    }

    private async Task SaveAndShowProfileImage(FileResult photo)
    {
        string fileName = $"profile_{Guid.NewGuid():N}.jpg";
        string destPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

        using (var sourceStream = await photo.OpenReadAsync())
        using (var destStream = File.OpenWrite(destPath))
        {
            await sourceStream.CopyToAsync(destStream);
        }

        ProfileImagePath = destPath;
        ImageSource = ImageSource.FromFile(destPath);
        HasImage = true;
    }

    private async Task ToggleFlashForCapture()
    {
        try
        {
            Flashlight.Default.TurnOnAsync().ConfigureAwait(false);
            await Task.Delay(500);
            Flashlight.Default.TurnOffAsync().ConfigureAwait(false);
        }
        catch { }
    }

    [RelayCommand]
    private async Task GetLocationAsync()
    {
        try
        {
            LocationText = "Fetching location...";
            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var location = await Geolocation.Default.GetLocationAsync(request);

            if (location != null)
            {
                Latitude = location.Latitude;
                Longitude = location.Longitude;

                var placemarks = await Geocoding.Default.GetPlacemarksAsync(location.Latitude, location.Longitude);
                var placemark = placemarks?.FirstOrDefault();

                if (placemark != null)
                {
                    LocationText = $"{placemark.Locality}, {placemark.CountryName}";
                }
                else
                {
                    LocationText = $"Lat: {location.Latitude:F2}, Lon: {location.Longitude:F2}";
                }
            }
            else
            {
                LocationText = "Could not determine location.";
            }
        }
        catch (Exception ex)
        {
            LocationText = "Failed to get location.";
            System.Diagnostics.Debug.WriteLine($"Location error: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var page = Shell.Current?.CurrentPage;
        if (page == null) return;

        if (string.IsNullOrWhiteSpace(Name))
        {
            try { HapticFeedback.Default.Perform(HapticFeedbackType.LongPress); } catch { }
            await Task.Delay(50);
            await page.DisplayAlert("Validation", "Name is required.", "OK");
            return;
        }
        if (string.IsNullOrEmpty(Gender))
        {
            try { HapticFeedback.Default.Perform(HapticFeedbackType.LongPress); } catch { }
            await Task.Delay(50);
            await page.DisplayAlert("Validation", "Please select a gender.", "OK");
            return;
        }
        if (!int.TryParse(Age, out int age) || age < 1 || age > 120)
        {
            try { HapticFeedback.Default.Perform(HapticFeedbackType.LongPress); } catch { }
            await Task.Delay(50);
            await page.DisplayAlert("Validation", "Please enter a valid age (1-120).", "OK");
            return;
        }
        if (!double.TryParse(Height, out double height) || height < 50 || height > 300)
        {
            try { HapticFeedback.Default.Perform(HapticFeedbackType.LongPress); } catch { }
            await Task.Delay(50);
            await page.DisplayAlert("Validation", "Please enter a valid height in cm (50-300).", "OK");
            return;
        }
        if (!double.TryParse(Weight, out double weight) || weight < 10 || weight > 500)
        {
            try { HapticFeedback.Default.Perform(HapticFeedbackType.LongPress); } catch { }
            await Task.Delay(50);
            await page.DisplayAlert("Validation", "Please enter a valid weight in kg (10-500).", "OK");
            return;
        }
        if (string.IsNullOrEmpty(ActivityLevel))
        {
            try { HapticFeedback.Default.Perform(HapticFeedbackType.LongPress); } catch { }
            await Task.Delay(50);
            await page.DisplayAlert("Validation", "Please select an activity level.", "OK");
            return;
        }
        if (string.IsNullOrEmpty(Goal))
        {
            try { HapticFeedback.Default.Perform(HapticFeedbackType.LongPress); } catch { }
            await Task.Delay(50);
            await page.DisplayAlert("Validation", "Please select a goal.", "OK");
            return;
        }

        var profile = new UserProfile
        {
            Name = Name.Trim(),
            Gender = Gender,
            Age = age,
            HeightCm = height,
            WeightKg = weight,
            ActivityLevel = ActivityLevel,
            Goal = Goal,
            Latitude = Latitude,
            Longitude = Longitude,
            LocationText = LocationText == "No location set." || LocationText == "Fetching location..." || LocationText == "Failed to get location." || LocationText == "Could not determine location." ? string.Empty : LocationText,
            ProfileImagePath = ProfileImagePath,
        };

        profile.DailyCalorieTarget = profile.CalculateDailyCalories();
        profile.LastCalculatedDate = DateTime.Today;

        ProfileService.AddProfile(profile);

        try { HapticFeedback.Default.Perform(HapticFeedbackType.Click); } catch { }

        await page.DisplayAlert("Profile Created!",
            $"Name: {profile.Name}\n" +
            $"Daily Target: {profile.DailyCalorieTarget} kcal\n" +
            $"BMR: {profile.CalculateBMR():N0} kcal\n" +
            $"Macro: {profile.MacroBreakdown}",
            "OK");

        await Shell.Current.GoToAsync("..");
    }
}
