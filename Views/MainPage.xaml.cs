using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Shapes;
using FoodDrinkApp.Models;
using FoodDrinkApp.Services;
using FoodDrinkApp.Helpers;

namespace FoodDrinkApp.Views;

public partial class MainPage : ContentPage
{
    private List<FoodItem> _allFoods = new();

    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
        await LoadDataAsync();
    }

    private async Task LoadDataAsync(string query = "")
    {
        FoodRefreshView.IsRefreshing = true;

        _allFoods = await FoodCatalogService.GetFoodsAsync(query);

        // Update summary card with today's diet progress
        var profile = ProfileService.GetActiveProfile();
        double consumed = DietRecordService.GetTodayTotalCalories();

        if (profile != null)
        {
            ProfileService.RefreshDailyCalories(profile);
            double remaining = profile.DailyCalorieTarget - consumed;
            TotalCountLabel.Text = $"{_allFoods.Count} foods available  ·  {profile.Name}";
            TotalCaloriesLabel.Text = remaining >= 0
                ? $"{consumed:N0} / {profile.DailyCalorieTarget:N0} kcal  ·  {remaining:N0} remaining"
                : $"{consumed:N0} / {profile.DailyCalorieTarget:N0} kcal  ·  {Math.Abs(remaining):N0} over!";
        }
        else
        {
            TotalCountLabel.Text = $"{_allFoods.Count} items in your collection";
            TotalCaloriesLabel.Text = "Set up a profile to track daily calories";
        }

        // Build category sections
        BuildCategorySections(_allFoods);

        FoodRefreshView.IsRefreshing = false;
    }

    /// <summary>
    /// Dynamically builds horizontal-scrolling category sections,
    /// matching the magazine-style layout of the reference image.
    /// </summary>
    private void BuildCategorySections(List<FoodItem> foods)
    {
        CategoriesContainer.Children.Clear();

        var grouped = foods.GroupBy(f => f.Category)
                          .OrderBy(g => FoodCatalogService.CategoryOrder(g.Key));

        foreach (var group in grouped)
        {
            var categoryName = group.Key;
            var subtitle = FoodCategoryGroup.GetSubtitleForCategory(categoryName);
            var items = group.ToList();

            // ── Section Header ──
            var headerGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto)
                },
                Padding = new Thickness(20, 15, 20, 5)
            };

            // Left: vertical bar + category title
            var titleLayout = new HorizontalStackLayout { Spacing = 8, VerticalOptions = LayoutOptions.Center };
            var verticalBar = new BoxView
            {
                WidthRequest = 4,
                HeightRequest = 22,
                CornerRadius = 2,
                Color = items.FirstOrDefault()?.CategoryBadgeColor ?? Colors.Gray,
                VerticalOptions = LayoutOptions.Center
            };
            titleLayout.Children.Add(verticalBar);
            titleLayout.Children.Add(new Label
            {
                Text = categoryName,
                FontSize = 20,
                FontAttributes = FontAttributes.Bold,
                FontFamily = "SegoeSemibold",
                CharacterSpacing = 0.3,
                VerticalOptions = LayoutOptions.Center
            });
            headerGrid.SetColumn(titleLayout, 0);
            headerGrid.Children.Add(titleLayout);

            // Right: subtitle
            var subtitleLabel = new Label
            {
                Text = subtitle,
                FontSize = 13,
                VerticalOptions = LayoutOptions.Center,
                TextColor = Application.Current?.RequestedTheme == AppTheme.Dark
                    ? Color.FromArgb("#9CA3AF") : Color.FromArgb("#6B7280")
            };
            headerGrid.SetColumn(subtitleLabel, 1);
            headerGrid.Children.Add(subtitleLabel);

            CategoriesContainer.Children.Add(headerGrid);

            // ── Horizontal scroll of food cards ──
            var scrollView = new ScrollView
            {
                Orientation = ScrollOrientation.Horizontal,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
                Padding = new Thickness(16, 4, 0, 8)
            };

            var cardRow = new HorizontalStackLayout { Spacing = 10 };

            foreach (var food in items)
            {
                cardRow.Children.Add(CreateFoodCard(food));
            }

            scrollView.Content = cardRow;
            CategoriesContainer.Children.Add(scrollView);
        }
    }

    /// <summary>
    /// Creates a visually rich food card with category badge, matching the reference design.
    /// </summary>
    private View CreateFoodCard(FoodItem food)
    {
        var cardBorder = new Border
        {
            WidthRequest = 156,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            StrokeThickness = 0,
            Padding = 0,
            BackgroundColor = Application.Current?.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#1C1C30") : Colors.White
        };
        cardBorder.Shadow = new Shadow
        {
            Brush = Brush.Black,
            Offset = new Point(0, 3),
            Opacity = 0.08f,
            Radius = 8
        };

        var cardContent = new VerticalStackLayout { Spacing = 0 };

        // ── Image area with category badge overlay ──
        var imageContainer = new Grid
        {
            HeightRequest = 110,
            WidthRequest = 156
        };

        // Placeholder background with category icon
        var placeholderBg = new BoxView
        {
            Color = food.CategoryBadgeColor.WithAlpha(0.15f),
            CornerRadius = new CornerRadius(14, 14, 0, 0)
        };
        imageContainer.Children.Add(placeholderBg);

        // Category icon in centre (FluentUI font)
        var iconLabel = new Label
        {
            Text = food.CategoryIcon,
            FontFamily = AppIcons.FontFamily,
            FontSize = 36,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            TextColor = food.CategoryBadgeColor
        };
        imageContainer.Children.Add(iconLabel);

        // If image is available, show the actual image (supports both URLs and local files)
        if (food.HasImage)
        {
            var foodImage = new Image
            {
                Source = food.ResolvedImageSource,
                Aspect = Aspect.AspectFill
            };
            imageContainer.Children.Add(foodImage);
        }

        // Category badge (top-left overlay)
        var badgeBorder = new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            StrokeThickness = 0,
            BackgroundColor = food.CategoryBadgeColor,
            Padding = new Thickness(8, 3),
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Start,
            Margin = new Thickness(8, 8, 0, 0)
        };
        badgeBorder.Content = new Label
        {
            Text = food.Category,
            FontSize = 10,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White
        };
        imageContainer.Children.Add(badgeBorder);

        cardContent.Children.Add(imageContainer);

        // ── Text area ──
        var textArea = new VerticalStackLayout
        {
            Padding = new Thickness(10, 8, 10, 10),
            Spacing = 2
        };

        textArea.Children.Add(new Label
        {
            Text = food.Name,
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            LineBreakMode = LineBreakMode.TailTruncation,
            MaxLines = 2
        });

        textArea.Children.Add(new Label
        {
            Text = food.CaloriesLabel,
            FontSize = 11,
            TextColor = Application.Current?.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#9CA3AF") : Color.FromArgb("#6B7280")
        });

        cardContent.Children.Add(textArea);
        cardBorder.Content = cardContent;

        // ── Tap gesture to navigate to detail ──
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += async (s, e) =>
        {
            // Subtle scale animation on tap
            await cardBorder.ScaleTo(0.95, 80, Easing.CubicIn);
            await cardBorder.ScaleTo(1.0, 80, Easing.CubicOut);

            var navigationParameter = new Dictionary<string, object> { { "Item", food } };
            await Shell.Current.GoToAsync("FoodDetailPage", navigationParameter);
        };
        cardBorder.GestureRecognizers.Add(tapGesture);

        return cardBorder;
    }

    private async void OnRefreshing(object? sender, EventArgs e)
    {
        FoodSearchBar.Text = string.Empty;
        await LoadDataAsync();
    }

    private async void OnSearchPressed(object? sender, EventArgs e)
    {
        var query = FoodSearchBar.Text ?? string.Empty;
        await LoadDataAsync(query);
    }

    private async void OnAddClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("AddItemPage");
    }
}