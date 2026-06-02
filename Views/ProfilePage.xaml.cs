using Microsoft.Maui.Controls.Shapes;
using FoodDrinkApp.Helpers;

namespace FoodDrinkApp.Views;

public partial class ProfilePage : ContentPage
{
    public ProfilePage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
        await ProfileService.SyncFromApiAsync();
        RefreshUI();
    }

    // ─── UI Refresh ───

    private void RefreshUI()
    {
        var profiles = ProfileService.GetAllProfiles();
        var active = ProfileService.GetActiveProfile();

        // Update dashboard
        if (active != null)
        {
            ProfileService.RefreshDailyCalories(active);

            ActiveProfileLabel.Text = $"{active.DisplayLabel}  •  {active.ActivityLevel}";
            CalorieTargetLabel.Text = active.DailyCalorieTarget.ToString("N0");
            GoalIconLabel.Text = active.GoalIcon;
            GoalLabel.Text = active.Goal;

            // Macro breakdown
            int cal = active.DailyCalorieTarget;
            int protein = (int)(cal * 0.30 / 4);
            int carbs = (int)(cal * 0.45 / 4);
            int fat = (int)(cal * 0.25 / 9);
            ProteinLabel.Text = $"{protein}g";
            CarbsLabel.Text = $"{carbs}g";
            FatLabel.Text = $"{fat}g";

            LocationLabel.Text = string.IsNullOrEmpty(active.LocationText)
                ? "No location set"
                : active.LocationText;
            RefreshDateLabel.Text = $"Updated: {active.LastCalculatedDate:MMM dd}";

            CalorieDashboard.IsVisible = true;
        }
        else
        {
            CalorieDashboard.IsVisible = false;
        }

        // Build profile cards
        ProfileListContainer.Children.Clear();
        EmptyState.IsVisible = profiles.Count == 0;

        foreach (var profile in profiles)
        {
            ProfileListContainer.Children.Add(CreateProfileCard(profile));
        }
    }

    // ─── Profile Card Builder ───

    private View CreateProfileCard(UserProfile profile)
    {
        var isActive = profile.IsActive;
        var accentColor = isActive ? Color.FromArgb("#7C4DFF") : Color.FromArgb("#607D8B");

        var card = new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            Stroke = isActive ? new SolidColorBrush(Color.FromArgb("#7C4DFF")) : new SolidColorBrush(Colors.Transparent),
            StrokeThickness = isActive ? 2 : 0,
            Padding = new Thickness(16),
            BackgroundColor = Application.Current?.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#1C1C30") : Colors.White,
        };
        card.Shadow = new Shadow
        {
            Brush = Brush.Black,
            Offset = new Point(0, 2),
            Opacity = 0.08f,
            Radius = 6
        };

        var mainGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(new GridLength(50)),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
            ColumnSpacing = 14,
        };

        // Avatar circle – show profile photo if available, else default icon
        var avatar = new Border
        {
            WidthRequest = 48,
            HeightRequest = 48,
            StrokeShape = new RoundRectangle { CornerRadius = 24 },
            StrokeThickness = 0,
            BackgroundColor = accentColor.WithAlpha(0.15f),
            VerticalOptions = LayoutOptions.Center,
        };

        bool hasImage = !string.IsNullOrEmpty(profile.ProfileImagePath)
                        && File.Exists(profile.ProfileImagePath);

        if (hasImage)
        {
            avatar.Content = new Image
            {
                Source = ImageSource.FromFile(profile.ProfileImagePath),
                Aspect = Aspect.AspectFill,
                WidthRequest = 48,
                HeightRequest = 48,
            };
        }
        else
        {
            avatar.Content = new Label
            {
                Text = AppIcons.Person,
                FontFamily = AppIcons.FontFamily,
                FontSize = 24,
                TextColor = accentColor,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
            };
        }
        Grid.SetColumn(avatar, 0);
        mainGrid.Children.Add(avatar);

        // Text info
        var textStack = new VerticalStackLayout { Spacing = 3, VerticalOptions = LayoutOptions.Center };
        textStack.Children.Add(new Label
        {
            Text = profile.Name,
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            FontFamily = "SegoeSemibold",
            TextColor = Application.Current?.RequestedTheme == AppTheme.Dark ? Colors.White : Color.FromArgb("#1A1A1A"),
        });

        var detailLayout = new HorizontalStackLayout { Spacing = 6 };
        detailLayout.Children.Add(new Label
        {
            Text = $"{profile.Age}y • {profile.WeightKg}kg • {profile.HeightCm}cm",
            FontSize = 12,
            TextColor = Application.Current?.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#9CA3AF") : Color.FromArgb("#6B7280"),
        });
        textStack.Children.Add(detailLayout);

        var goalIconLabel = new Label
        {
            Text = profile.GoalIcon,
            FontFamily = AppIcons.FontFamily,
            FontSize = 14,
            TextColor = accentColor,
            VerticalOptions = LayoutOptions.Center,
        };
        var goalTextLabel = new Label
        {
            Text = $"{profile.Goal} · {profile.DailyCalorieTarget} kcal/day",
            FontSize = 12,
            TextColor = accentColor,
            FontAttributes = FontAttributes.Bold,
        };
        var goalRow = new HorizontalStackLayout { Spacing = 6 };
        goalRow.Children.Add(goalIconLabel);
        goalRow.Children.Add(goalTextLabel);
        textStack.Children.Add(goalRow);

        if (isActive)
        {
            var activeBadge = new Border
            {
                StrokeShape = new RoundRectangle { CornerRadius = 8 },
                StrokeThickness = 0,
                BackgroundColor = Color.FromArgb("#4CAF50"),
                Padding = new Thickness(8, 2),
                HorizontalOptions = LayoutOptions.Start,
                Margin = new Thickness(0, 3, 0, 0),
            };
            var badgeContent = new HorizontalStackLayout { Spacing = 4 };
            badgeContent.Children.Add(new Label
            {
                Text = AppIcons.Checkmark,
                FontFamily = AppIcons.FontFamily,
                FontSize = 10,
                TextColor = Colors.White,
                VerticalOptions = LayoutOptions.Center,
            });
            badgeContent.Children.Add(new Label
            {
                Text = "Active",
                FontSize = 10,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
            });
            activeBadge.Content = badgeContent;
            textStack.Children.Add(activeBadge);
        }

        Grid.SetColumn(textStack, 1);
        mainGrid.Children.Add(textStack);

        // Action buttons
        var actionStack = new VerticalStackLayout { Spacing = 6, VerticalOptions = LayoutOptions.Center };

        if (!isActive)
        {
            var switchBtn = new Border
            {
                StrokeShape = new RoundRectangle { CornerRadius = 10 },
                StrokeThickness = 0,
                BackgroundColor = Color.FromArgb("#7C4DFF"),
                Padding = new Thickness(10, 6),
                HorizontalOptions = LayoutOptions.End,
            };
            switchBtn.Content = new Label { Text = "Switch", FontSize = 11, TextColor = Colors.White, FontAttributes = FontAttributes.Bold };
            var switchTap = new TapGestureRecognizer();
            var profileId = profile.Id;
            switchTap.Tapped += (s, e) =>
            {
                ProfileService.SetActiveProfile(profileId);
                // Immediately calculate the new profile's calorie target for today
                var newActive = ProfileService.GetActiveProfile();
                if (newActive != null) ProfileService.RefreshDailyCalories(newActive);
                RefreshUI();
            };
            switchBtn.GestureRecognizers.Add(switchTap);
            actionStack.Children.Add(switchBtn);
        }

        var deleteBtn = new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = 10 },
            StrokeThickness = 0,
            BackgroundColor = Color.FromArgb("#F44336").WithAlpha(0.12f),
            Padding = new Thickness(10, 6),
            HorizontalOptions = LayoutOptions.End,
        };
        deleteBtn.Content = new Label
        {
            Text = AppIcons.Delete,
            FontFamily = AppIcons.FontFamily,
            FontSize = 16,
            TextColor = Color.FromArgb("#F44336"),
            HorizontalOptions = LayoutOptions.Center,
        };
        var deleteTap = new TapGestureRecognizer();
        var delId = profile.Id;
        var delName = profile.Name;
        deleteTap.Tapped += async (s, e) =>
        {
            bool confirm = await DisplayAlert("Delete Profile", $"Delete '{delName}'?", "Yes", "Cancel");
            if (confirm)
            {
                ProfileService.DeleteProfile(delId);
                RefreshUI();
            }
        };
        deleteBtn.GestureRecognizers.Add(deleteTap);
        actionStack.Children.Add(deleteBtn);

        Grid.SetColumn(actionStack, 2);
        mainGrid.Children.Add(actionStack);

        card.Content = mainGrid;
        return card;
    }

    // ─── Navigate to the dedicated form page ───

    private async void OnAddProfileClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("EditProfilePage");
    }
}
