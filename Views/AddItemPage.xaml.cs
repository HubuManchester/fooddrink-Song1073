using FoodDrinkApp.Models;
using FoodDrinkApp.Services;

namespace FoodDrinkApp.Views;

public partial class AddItemPage : ContentPage
{
    private string _currentImagePath = string.Empty;
    private string _currentLocation = string.Empty;

    public AddItemPage()
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

    private async void OnTakePhotoClicked(object? sender, EventArgs e)
    {
        try
        {
            if (MediaPicker.Default.IsCaptureSupported)
            {
                var photo = await MediaPicker.Default.CapturePhotoAsync();
                if (photo != null)
                {
                    string localFilePath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
                    using Stream sourceStream = await photo.OpenReadAsync();
                    using FileStream localFileStream = File.OpenWrite(localFilePath);
                    await sourceStream.CopyToAsync(localFileStream);

                    _currentImagePath = localFilePath;
                    FoodImageView.Source = ImageSource.FromFile(localFilePath);
                    FoodImageView.IsVisible = true;
                    DefaultFoodIcon.IsVisible = false;
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Camera Error", ex.Message, "OK");
        }
    }

    private async void OnGetLocationClicked(object? sender, EventArgs e)
    {
        try
        {
            LocationLabel.Text = "Fetching location...";
            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var location = await Geolocation.Default.GetLocationAsync(request);

            if (location != null)
            {
                var placemarks = await Geocoding.Default.GetPlacemarksAsync(location.Latitude, location.Longitude);
                var placemark = placemarks?.FirstOrDefault();

                if (placemark != null)
                {
                    _currentLocation = $"{placemark.Locality}, {placemark.CountryName}";
                }
                else
                {
                    _currentLocation = $"Lat: {location.Latitude:F2}, Lon: {location.Longitude:F2}";
                }
                LocationLabel.Text = _currentLocation;
                LocationLabel.TextColor = Colors.DarkBlue;
            }
        }
        catch (Exception ex)
        {
            LocationLabel.Text = "Failed to get location.";
            await DisplayAlertAsync("Location Error", ex.Message, "OK");
        }
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameEntry.Text) || CategoryPicker.SelectedItem == null)
        {
            try { Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(250)); } catch { }
            await DisplayAlertAsync("Validation Error", "Name and Category are required.", "OK");
            return;
        }

        if (!double.TryParse(CaloriesEntry.Text, out double calories) || calories < 0)
        {
            try { Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(250)); } catch { }
            await DisplayAlertAsync("Validation Error", "Calories must be a valid non-negative number.", "OK");
            return;
        }

        double.TryParse(ProteinEntry.Text, out double protein);
        double.TryParse(CarbsEntry.Text, out double carbs);
        double.TryParse(FatEntry.Text, out double fat);

        var newItem = new FoodItem
        {
            Name = NameEntry.Text.Trim(),
            Category = CategoryPicker.SelectedItem.ToString() ?? "Other",
            Calories = calories,
            Protein = protein,
            Carbs = carbs,
            Fat = fat,
            Description = DescriptionEditor.Text?.Trim() ?? string.Empty,
            AllergyNote = AllergyEntry.Text?.Trim() ?? string.Empty,
            Tags = TagsEntry.Text?.Trim() ?? string.Empty,
            ImagePath = _currentImagePath,
            LocationText = _currentLocation
        };

        await FoodCatalogService.AddFoodAsync(newItem);
        await Shell.Current.GoToAsync("..");
    }
}