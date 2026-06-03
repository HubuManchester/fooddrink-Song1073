using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodDrinkApp.Models;
using FoodDrinkApp.Services;

namespace FoodDrinkApp.ViewModels;

public partial class AddItemViewModel : BaseViewModel
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _category = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _calories = string.Empty;

    [ObservableProperty]
    private string _protein = string.Empty;

    [ObservableProperty]
    private string _carbs = string.Empty;

    [ObservableProperty]
    private string _fat = string.Empty;

    [ObservableProperty]
    private string _allergyNote = string.Empty;

    [ObservableProperty]
    private string _tags = string.Empty;

    [ObservableProperty]
    private string _currentImagePath = string.Empty;

    [ObservableProperty]
    private string _currentLocationText = "No location tagged.";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoImage))]
    private bool _hasImage;

    public bool HasNoImage => !HasImage;

    [ObservableProperty]
    private ImageSource? _imageSource;

    public List<string> Categories { get; } = new()
    {
        "Grain", "Meat", "Vegetable", "Fruit", "Dairy", "Seafood", "Beverage", "Dessert"
    };

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task TakePhotoAsync()
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

                    CurrentImagePath = localFilePath;
                    ImageSource = ImageSource.FromFile(localFilePath);
                    HasImage = true;
                }
            }
        }
        catch (Exception ex)
        {
            if (Shell.Current?.CurrentPage != null)
                await Shell.Current.CurrentPage.DisplayAlert("Camera Error", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task GetLocationAsync()
    {
        try
        {
            CurrentLocationText = "Fetching location...";
            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var location = await Geolocation.Default.GetLocationAsync(request);

            if (location != null)
            {
                var placemarks = await Geocoding.Default.GetPlacemarksAsync(location.Latitude, location.Longitude);
                var placemark = placemarks?.FirstOrDefault();

                if (placemark != null)
                {
                    CurrentLocationText = $"{placemark.Locality}, {placemark.CountryName}";
                }
                else
                {
                    CurrentLocationText = $"Lat: {location.Latitude:F2}, Lon: {location.Longitude:F2}";
                }
            }
            else
            {
                CurrentLocationText = "Failed to get location.";
            }
        }
        catch (Exception ex)
        {
            CurrentLocationText = "Failed to get location.";
            if (Shell.Current?.CurrentPage != null)
                await Shell.Current.CurrentPage.DisplayAlert("Location Error", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Category))
        {
            try { Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(250)); } catch { }
            if (Shell.Current?.CurrentPage != null)
                await Shell.Current.CurrentPage.DisplayAlert("Validation Error", "Name and Category are required.", "OK");
            return;
        }

        if (!double.TryParse(Calories, out double calories) || calories < 0)
        {
            try { Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(250)); } catch { }
            if (Shell.Current?.CurrentPage != null)
                await Shell.Current.CurrentPage.DisplayAlert("Validation Error", "Calories must be a valid non-negative number.", "OK");
            return;
        }

        double.TryParse(Protein, out double protein);
        double.TryParse(Carbs, out double carbs);
        double.TryParse(Fat, out double fat);

        string locationToSave = CurrentLocationText == "No location tagged." || CurrentLocationText == "Fetching location..." || CurrentLocationText == "Failed to get location." 
            ? string.Empty : CurrentLocationText;

        var newItem = new FoodItem
        {
            Name = Name.Trim(),
            Category = Category,
            Calories = calories,
            Protein = protein,
            Carbs = carbs,
            Fat = fat,
            Description = Description?.Trim() ?? string.Empty,
            AllergyNote = AllergyNote?.Trim() ?? string.Empty,
            Tags = Tags?.Trim() ?? string.Empty,
            ImagePath = CurrentImagePath,
            LocationText = locationToSave
        };

        await FoodCatalogService.AddFoodAsync(newItem);
        await Shell.Current.GoToAsync("..");
    }
}
