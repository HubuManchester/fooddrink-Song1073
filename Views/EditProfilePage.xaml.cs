namespace FoodDrinkApp.Views;

public partial class EditProfilePage : ContentPage
{
    private double _latitude;
    private double _longitude;
    private string _locationText = string.Empty;
    private string _profileImagePath = string.Empty;

    public EditProfilePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    // ─── Profile Photo ───

    /// <summary>
    /// Captures a photo with the camera (flash enabled) and uses it as the profile avatar.
    /// </summary>
    private async void OnTakeProfilePhotoClicked(object? sender, EventArgs e)
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                await DisplayAlert("Not Supported", "Camera is not available on this device.", "OK");
                return;
            }

            // Enable flash via platform-specific intent (handled by MediaPicker on supported devices)
            var photo = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
            {
                Title = "Take Profile Photo"
            });

            if (photo != null)
            {
                // Turn on flashlight briefly to simulate flash during capture
                await ToggleFlashForCapture();
                await SaveAndShowProfileImage(photo);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not take photo: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// Picks an existing photo from the device gallery.
    /// </summary>
    private async void OnPickProfilePhotoClicked(object? sender, EventArgs e)
    {
        try
        {
            var photo = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Select Profile Photo"
            });

            if (photo != null)
            {
                await SaveAndShowProfileImage(photo);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not pick photo: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// Removes the selected profile photo and reverts to the default icon.
    /// </summary>
    /// </summary>
    private async void OnRemoveProfilePhotoClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_profileImagePath))
        {
            try { HapticFeedback.Default.Perform(HapticFeedbackType.LongPress); } catch { }
            await Task.Delay(50); // Ensure haptics play before the alert blocks the UI
            await DisplayAlert("Error", "No photo to remove.", "OK");
            return;
        }

        _profileImagePath = string.Empty;
        ProfileImagePreview.Source = null;
        ProfileImagePreview.IsVisible = false;
        DefaultAvatarIcon.IsVisible = true;
    }

    /// <summary>
    /// Saves the captured/picked photo to app data and updates the preview.
    /// </summary>
    private async Task SaveAndShowProfileImage(FileResult photo)
    {
        // Copy to app data directory with a unique name
        string fileName = $"profile_{Guid.NewGuid():N}.jpg";
        string destPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

        using (var sourceStream = await photo.OpenReadAsync())
        using (var destStream = File.OpenWrite(destPath))
        {
            await sourceStream.CopyToAsync(destStream);
        }

        _profileImagePath = destPath;

        // Update preview
        ProfileImagePreview.Source = ImageSource.FromFile(destPath);
        ProfileImagePreview.IsVisible = true;
        DefaultAvatarIcon.IsVisible = false;
    }

    /// <summary>
    /// Briefly toggles the device flashlight to provide a flash effect during photo capture.
    /// </summary>
    private async Task ToggleFlashForCapture()
    {
        try
        {
            Flashlight.Default.TurnOnAsync().ConfigureAwait(false);
            await Task.Delay(500);
            Flashlight.Default.TurnOffAsync().ConfigureAwait(false);
        }
        catch
        {
            // Flashlight may not be available on all devices – silently ignore
        }
    }

    // ─── Location ───

    private async void OnGetLocationClicked(object? sender, EventArgs e)
    {
        try
        {
            LocationLabel.Text = "Fetching location...";
            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var location = await Geolocation.Default.GetLocationAsync(request);

            if (location != null)
            {
                _latitude = location.Latitude;
                _longitude = location.Longitude;

                var placemarks = await Geocoding.Default.GetPlacemarksAsync(location.Latitude, location.Longitude);
                var placemark = placemarks?.FirstOrDefault();

                if (placemark != null)
                {
                    _locationText = $"{placemark.Locality}, {placemark.CountryName}";
                }
                else
                {
                    _locationText = $"Lat: {location.Latitude:F2}, Lon: {location.Longitude:F2}";
                }
                LocationLabel.Text = _locationText;
                LocationLabel.TextColor = Color.FromArgb("#2196F3");
            }
            else
            {
                LocationLabel.Text = "Could not determine location.";
            }
        }
        catch (Exception ex)
        {
            LocationLabel.Text = "Failed to get location.";
            System.Diagnostics.Debug.WriteLine($"Location error: {ex.Message}");
        }
    }

    // ─── Save ───

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        // ── Validation ──
        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            try { HapticFeedback.Default.Perform(HapticFeedbackType.LongPress); } catch { }
            await Task.Delay(50);
            await DisplayAlert("Validation", "Name is required.", "OK");
            return;
        }
        if (GenderPicker.SelectedItem == null)
        {
            try { HapticFeedback.Default.Perform(HapticFeedbackType.LongPress); } catch { }
            await Task.Delay(50);
            await DisplayAlert("Validation", "Please select a gender.", "OK");
            return;
        }
        if (!int.TryParse(AgeEntry.Text, out int age) || age < 1 || age > 120)
        {
            try { HapticFeedback.Default.Perform(HapticFeedbackType.LongPress); } catch { }
            await Task.Delay(50);
            await DisplayAlert("Validation", "Please enter a valid age (1-120).", "OK");
            return;
        }
        if (!double.TryParse(HeightEntry.Text, out double height) || height < 50 || height > 300)
        {
            try { HapticFeedback.Default.Perform(HapticFeedbackType.LongPress); } catch { }
            await Task.Delay(50);
            await DisplayAlert("Validation", "Please enter a valid height in cm (50-300).", "OK");
            return;
        }
        if (!double.TryParse(WeightEntry.Text, out double weight) || weight < 10 || weight > 500)
        {
            try { HapticFeedback.Default.Perform(HapticFeedbackType.LongPress); } catch { }
            await Task.Delay(50);
            await DisplayAlert("Validation", "Please enter a valid weight in kg (10-500).", "OK");
            return;
        }
        if (ActivityPicker.SelectedItem == null)
        {
            try { HapticFeedback.Default.Perform(HapticFeedbackType.LongPress); } catch { }
            await Task.Delay(50);
            await DisplayAlert("Validation", "Please select an activity level.", "OK");
            return;
        }
        if (GoalPicker.SelectedItem == null)
        {
            try { HapticFeedback.Default.Perform(HapticFeedbackType.LongPress); } catch { }
            await Task.Delay(50);
            await DisplayAlert("Validation", "Please select a goal.", "OK");
            return;
        }

        // ── Create profile ──
        var profile = new UserProfile
        {
            Name = NameEntry.Text.Trim(),
            Gender = GenderPicker.SelectedItem.ToString() ?? "Male",
            Age = age,
            HeightCm = height,
            WeightKg = weight,
            ActivityLevel = ActivityPicker.SelectedItem.ToString() ?? "Moderately Active",
            Goal = GoalPicker.SelectedItem.ToString() ?? "Maintain",
            Latitude = _latitude,
            Longitude = _longitude,
            LocationText = _locationText,
            ProfileImagePath = _profileImagePath,
        };

        // Calculate and set daily calorie target
        profile.DailyCalorieTarget = profile.CalculateDailyCalories();
        profile.LastCalculatedDate = DateTime.Today;

        // Save to service
        ProfileService.AddProfile(profile);

        try { HapticFeedback.Default.Perform(HapticFeedbackType.Click); } catch { }

        await DisplayAlert("Profile Created!",
            $"Name: {profile.Name}\n" +
            $"Daily Target: {profile.DailyCalorieTarget} kcal\n" +
            $"BMR: {profile.CalculateBMR():N0} kcal\n" +
            $"Macro: {profile.MacroBreakdown}",
            "OK");

        // Go back to profile page
        await Shell.Current.GoToAsync("..");
    }
}
