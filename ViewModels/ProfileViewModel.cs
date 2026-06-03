using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodDrinkApp.Models;
using FoodDrinkApp.Services;

namespace FoodDrinkApp.ViewModels;

public partial class ProfileCardViewModel : ObservableObject
{
    [ObservableProperty]
    private UserProfile _profile;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _detailsText = string.Empty;

    [ObservableProperty]
    private string _goalIcon = string.Empty;

    [ObservableProperty]
    private string _goalText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotActive))]
    private bool _isActive;

    public bool IsNotActive => !IsActive;

    [ObservableProperty]
    private Color _accentColor = Colors.Gray;

    [ObservableProperty]
    private Color _cardStroke = Colors.Transparent;

    [ObservableProperty]
    private double _cardStrokeThickness;

    [ObservableProperty]
    private bool _hasImage;

    [ObservableProperty]
    private ImageSource? _imageSource;

    [ObservableProperty]
    private bool _hasNoImage;
}

public partial class ProfileViewModel : BaseViewModel
{
    [ObservableProperty]
    private bool _hasActiveProfile;

    [ObservableProperty]
    private string _activeProfileLabel = "No profile set";

    [ObservableProperty]
    private string _calorieTargetText = "—";

    [ObservableProperty]
    private string _goalIcon = "\ue11c";

    [ObservableProperty]
    private string _goalLabel = "Maintain";

    [ObservableProperty]
    private string _proteinText = "—";

    [ObservableProperty]
    private string _carbsText = "—";

    [ObservableProperty]
    private string _fatText = "—";

    [ObservableProperty]
    private string _locationText = "No location set";

    [ObservableProperty]
    private string _refreshDateText = "";

    [ObservableProperty]
    private bool _isEmpty;

    [ObservableProperty]
    private ObservableCollection<ProfileCardViewModel> _profiles = new();

    public async Task InitializeAsync()
    {
        await ProfileService.SyncFromApiAsync();
        Refresh();
    }

    [RelayCommand]
    private void Refresh()
    {
        var profiles = ProfileService.GetAllProfiles();
        var active = ProfileService.GetActiveProfile();

        if (active != null)
        {
            ProfileService.RefreshDailyCalories(active);

            ActiveProfileLabel = $"{active.DisplayLabel}  •  {active.ActivityLevel}";
            CalorieTargetText = active.DailyCalorieTarget.ToString("N0");
            GoalIcon = active.GoalIcon;
            GoalLabel = active.Goal;

            int cal = active.DailyCalorieTarget;
            int protein = (int)(cal * 0.30 / 4);
            int carbs = (int)(cal * 0.45 / 4);
            int fat = (int)(cal * 0.25 / 9);

            ProteinText = $"{protein}g";
            CarbsText = $"{carbs}g";
            FatText = $"{fat}g";

            LocationText = string.IsNullOrEmpty(active.LocationText) ? "No location set" : active.LocationText;
            RefreshDateText = $"Updated: {active.LastCalculatedDate:MMM dd}";

            HasActiveProfile = true;
        }
        else
        {
            HasActiveProfile = false;
        }

        IsEmpty = profiles.Count == 0;
        
        var newProfiles = new ObservableCollection<ProfileCardViewModel>();
        foreach (var p in profiles)
        {
            bool isActive = p.IsActive;
            Color accentColor = isActive ? Color.FromArgb("#7C4DFF") : Color.FromArgb("#607D8B");
            bool hasImg = !string.IsNullOrEmpty(p.ProfileImagePath) && File.Exists(p.ProfileImagePath);

            newProfiles.Add(new ProfileCardViewModel
            {
                Profile = p,
                Name = p.Name,
                DetailsText = $"{p.Age}y • {p.WeightKg}kg • {p.HeightCm}cm",
                GoalIcon = p.GoalIcon,
                GoalText = $"{p.Goal} · {p.DailyCalorieTarget} kcal/day",
                IsActive = isActive,
                AccentColor = accentColor,
                CardStroke = isActive ? new SolidColorBrush(Color.FromArgb("#7C4DFF")).Color : Colors.Transparent,
                CardStrokeThickness = isActive ? 2 : 0,
                HasImage = hasImg,
                HasNoImage = !hasImg,
                ImageSource = hasImg ? ImageSource.FromFile(p.ProfileImagePath) : null
            });
        }
        Profiles = newProfiles;
    }

    [RelayCommand]
    private async Task AddProfileAsync()
    {
        await Shell.Current.GoToAsync("EditProfilePage");
    }

    [RelayCommand]
    private void SwitchProfile(ProfileCardViewModel vm)
    {
        ProfileService.SetActiveProfile(vm.Profile.Id);
        var newActive = ProfileService.GetActiveProfile();
        if (newActive != null) ProfileService.RefreshDailyCalories(newActive);
        Refresh();
    }

    [RelayCommand]
    private async Task DeleteProfileAsync(ProfileCardViewModel vm)
    {
        if (Shell.Current?.CurrentPage == null) return;
        bool confirm = await Shell.Current.CurrentPage.DisplayAlert("Delete Profile", $"Delete '{vm.Name}'?", "Yes", "Cancel");
        if (confirm)
        {
            ProfileService.DeleteProfile(vm.Profile.Id);
            Refresh();
        }
    }
}
