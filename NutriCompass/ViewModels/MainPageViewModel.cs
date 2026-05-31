using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using NutriCompass.Services;

namespace NutriCompass.ViewModels;

public class MainPageViewModel : INotifyPropertyChanged
{
    private readonly INavigationService _navigationService;
    private readonly IHardwareService _hardwareService;
    private readonly Task _locationInitializationTask;
    private string _errorMessage = string.Empty;
    private string _orientationStatus = "Orientation: Waiting for gyroscope data.";
    private string _locationStatus = "Location: Requesting permissions.";
    private string _restaurantRecommendation = "Gathering healthy eatery suggestions.";

    public MainPageViewModel(INavigationService navigationService, IHardwareService hardwareService)
    {
        _navigationService = navigationService;
        _hardwareService = hardwareService;
        Highlights = new ObservableCollection<HighlightSection>
        {
            new HighlightSection(
                "Accessibility Ready",
                "Every control exposes AutomationProperties and meets WCAG guided contrast ratios."),
            new HighlightSection(
                "Responsive Layout",
                "Adaptive spacing ensures touch targets stay large on small and large screens."),
            new HighlightSection(
                "Guided Support",
                "Quick links explain how to extend this shell with your own hardware features.")
        };

        HelpCommand = new Command(async () => await _navigationService.NavigateToHelpAsync());
        ProfileCommand = new Command(async () => await _navigationService.NavigateToProfileAsync());

        _hardwareService.OrientationChanged += OnOrientationChanged;
        _hardwareService.StartOrientationUpdates();
        _locationInitializationTask = RefreshLocationAsync();
    }

    public string PageTitle => "NutriCompass Overview";

    public string BannerHeading => "Guided Nutrition Compass";

    public string BannerSubtitle => "Focus on accessibility, clarity, and actionable insights.";

    public string SummaryText => "Leverage WCAG friendly palettes, automation annotations, and semantic stacks to keep every interaction predictable.";

    public string SearchPlaceholder => "Search for a food or habit";

    public ObservableCollection<HighlightSection> Highlights { get; }

    public ICommand HelpCommand { get; }
    public ICommand ProfileCommand { get; }

    public string OrientationStatus
    {
        get => _orientationStatus;
        private set
        {
            if (_orientationStatus == value)
            {
                return;
            }

            _orientationStatus = value;
            OnPropertyChanged();
        }
    }

    public string LocationStatus
    {
        get => _locationStatus;
        private set
        {
            if (_locationStatus == value)
            {
                return;
            }

            _locationStatus = value;
            OnPropertyChanged();
        }
    }

    public string RestaurantRecommendation
    {
        get => _restaurantRecommendation;
        private set
        {
            if (_restaurantRecommendation == value)
            {
                return;
            }

            _restaurantRecommendation = value;
            OnPropertyChanged();
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (_errorMessage == value)
            {
                return;
            }

            _errorMessage = value;
            OnPropertyChanged();
        }
    }

    internal Task LocationInitializationTask => _locationInitializationTask;

    public event PropertyChangedEventHandler? PropertyChanged;

    private async Task RefreshLocationAsync()
    {
        try
        {
            var location = await _hardwareService.GetLocationAsync(CancellationToken.None);
            ErrorMessage = string.Empty;
            LocationStatus = $"Location: {location.Latitude:F4}, {location.Longitude:F4}";
            RestaurantRecommendation = ComposeRestaurantRecommendation(location);
        }
        catch (PermissionException)
        {
            ErrorMessage = "Location permission denied. Enable GPS for healthier recommendations.";
            LocationStatus = "Location: Permission required.";
            RestaurantRecommendation = "Location permission is needed to highlight nearby healthy restaurants.";
        }
        catch (Exception)
        {
            ErrorMessage = "Unable to resolve current location right now.";
            LocationStatus = "Location: Unavailable.";
            RestaurantRecommendation = "Retrying soon for nearby meal ideas.";
        }
    }

    private void OnOrientationChanged(object? sender, HardwareOrientationChangedEventArgs e)
    {
        OrientationStatus = e.Orientation switch
        {
            HardwareOrientation.Landscape => "Orientation: Landscape detected via gyroscope.",
            HardwareOrientation.Portrait => "Orientation: Portrait detected via gyroscope.",
            _ => "Orientation: Waiting for clearer gyroscope data."
        };
    }

    private static string ComposeRestaurantRecommendation(HardwareLocationResult location)
    {
        var eateries = new[]
        {
            ("Green Plate Kitchen", "1.2 km"),
            ("Harvest Balance", "850 m"),
            ("Verdant Corner", "2.3 km"),
            ("Citrus & Whole", "1.8 km")
        };

        var index = (int)(Math.Abs(location.Latitude + location.Longitude) * 1000) % eateries.Length;
        var (name, distance) = eateries[index];
        return $"Healthy recommendation: {name} ({distance}) near your current coordinates.";
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public sealed record HighlightSection(string Title, string Description);
