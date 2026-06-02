using Microsoft.Maui.Controls.Shapes;
using FoodDrinkApp.Helpers;

namespace FoodDrinkApp.Views;

public partial class DietRecordPage : ContentPage
{
    public DietRecordPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
        RefreshUI();
    }

    private void OnRefreshing(object? sender, EventArgs e)
    {
        RefreshUI();
        DietRefreshView.IsRefreshing = false;
    }

    // ─── Quick Add Food ───
    // Shows a list of available foods from the catalog, then adds to today's record.
    private async void OnAddFoodClicked(object? sender, EventArgs e)
    {
        // Step 1: Select meal type
        string? mealType = await DisplayActionSheet("Which meal?", "Cancel", null,
            "Breakfast", "Lunch", "Dinner", "Snack");
        if (string.IsNullOrEmpty(mealType) || mealType == "Cancel") return;

        // Step 2: Get all foods from catalog and let user pick
        var allFoods = await FoodCatalogService.GetFoodsAsync();
        if (allFoods.Count == 0)
        {
            await DisplayAlert("No Foods", "No food items available. Add some from the Home page first.", "OK");
            return;
        }

        // Build names array for the picker
        var foodNames = allFoods.Select(f => $"{f.Name} ({f.Category} · {f.Calories} kcal)").ToArray();
        string? selectedFoodName = await DisplayActionSheet("Select a food", "Cancel", null, foodNames);
        if (string.IsNullOrEmpty(selectedFoodName) || selectedFoodName == "Cancel") return;

        // Find the matching food item
        int selectedIndex = Array.IndexOf(foodNames, selectedFoodName);
        if (selectedIndex < 0 || selectedIndex >= allFoods.Count) return;
        var selectedFood = allFoods[selectedIndex];

        // Step 3: Ask servings (with a default of 1)
        string? servingsStr = await DisplayPromptAsync("Servings",
            $"How many servings of {selectedFood.Name}?",
            accept: "Add", cancel: "Cancel",
            placeholder: "1", maxLength: 5,
            keyboard: Keyboard.Numeric,
            initialValue: "1");

        if (string.IsNullOrWhiteSpace(servingsStr)) return;
        if (!double.TryParse(servingsStr, out double servings) || servings <= 0)
        {
            await DisplayAlert("Invalid", "Servings must be a positive number.", "OK");
            return;
        }

        // Step 4: Create and save the record
        var record = DietRecord.FromFoodItem(selectedFood, mealType, servings);
        DietRecordService.AddRecord(record);

        try { HapticFeedback.Default.Perform(HapticFeedbackType.Click); } catch { }

        // Refresh UI to show the new record
        RefreshUI();

        await DisplayAlert("Added!",
            $"'{selectedFood.Name}' × {servings:N1}\n" +
            $"Added to {mealType}\n" +
            $"Total: {record.TotalCalories:N0} kcal",
            "OK");
    }

    // ─── UI Refresh ───

    private void RefreshUI()
    {
        DateLabel.Text = DateTime.Today.ToString("dddd, MMMM dd, yyyy");

        var profile = ProfileService.GetActiveProfile();
        double consumed = DietRecordService.GetTodayTotalCalories();
        var (protein, carbs, fat) = DietRecordService.GetTodayMacros();

        int target = 0;
        if (profile != null)
        {
            ProfileService.RefreshDailyCalories(profile);
            target = profile.DailyCalorieTarget;
        }

        // Update summary card
        ConsumedLabel.Text = consumed.ToString("N0");
        TargetLabel.Text = target > 0 ? $"{target:N0} kcal" : "Set up a Profile first";
        TodayProteinLabel.Text = $"{protein:N0}g";
        TodayCarbsLabel.Text = $"{carbs:N0}g";

        double remaining = target - consumed;
        RemainingLabel.Text = target > 0 ? $"{remaining:N0}" : "—";
        RemainingLabel.TextColor = remaining >= 0
            ? Color.FromArgb("#4CAF50")
            : Color.FromArgb("#F44336");

        // Progress bar
        if (target > 0)
        {
            double ratio = Math.Clamp(consumed / target, 0, 1.0);
            // Calculate width using approximate parent width
            var parentWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density - 80;
            if (parentWidth < 100) parentWidth = 300; // safety minimum
            ProgressBarFill.WidthRequest = parentWidth * ratio;
            ProgressBarFill.BackgroundColor = remaining >= 0
                ? Color.FromArgb("#4CAF50")
                : Color.FromArgb("#F44336");
        }
        else
        {
            ProgressBarFill.WidthRequest = 0;
        }

        // Build meal sections
        BuildMealSections();
    }

    private void BuildMealSections()
    {
        MealSectionsContainer.Children.Clear();

        var grouped = DietRecordService.GetTodayGroupedByMeal();
        EmptyState.IsVisible = grouped.Count == 0;

        var mealIcons = new Dictionary<string, string>
        {
            { "Breakfast", "\uf1ec" },
            { "Lunch", "\ueee8" },
            { "Dinner", "\uf1ec" },
            { "Snack", "\uf1ec" },
        };

        var mealColors = new Dictionary<string, string>
        {
            { "Breakfast", "#FF6B35" },
            { "Lunch", "#4CAF50" },
            { "Dinner", "#E91E63" },
            { "Snack", "#FF9800" },
        };

        foreach (var meal in grouped)
        {
            var mealName = meal.Key;
            var items = meal.Value;
            var icon = mealIcons.GetValueOrDefault(mealName, "\uf1ec");
            var colorHex = mealColors.GetValueOrDefault(mealName, "#607D8B");
            var mealCalories = items.Sum(r => r.TotalCalories);

            // Section card
            var sectionBorder = new Border
            {
                StrokeShape = new RoundRectangle { CornerRadius = 16 },
                StrokeThickness = 0,
                Padding = new Thickness(16),
                BackgroundColor = Application.Current?.RequestedTheme == AppTheme.Dark
                    ? Color.FromArgb("#1C1C30") : Colors.White,
            };
            sectionBorder.Shadow = new Shadow
            {
                Brush = Brush.Black,
                Offset = new Point(0, 1),
                Opacity = 0.06f,
                Radius = 4
            };

            var sectionContent = new VerticalStackLayout { Spacing = 10 };

            // Meal header
            var headerGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto),
                },
            };

            var headerLeft = new HorizontalStackLayout { Spacing = 8, VerticalOptions = LayoutOptions.Center };
            var verticalBar = new BoxView
            {
                WidthRequest = 4,
                HeightRequest = 20,
                CornerRadius = 2,
                Color = Color.FromArgb(colorHex),
                VerticalOptions = LayoutOptions.Center,
            };
            headerLeft.Children.Add(verticalBar);
            // FluentUI icon label
            headerLeft.Children.Add(new Label
            {
                Text = icon,
                FontFamily = AppIcons.FontFamily,
                FontSize = 18,
                TextColor = Color.FromArgb(colorHex),
                VerticalOptions = LayoutOptions.Center,
            });
            // Meal name text label
            headerLeft.Children.Add(new Label
            {
                Text = mealName,
                FontSize = 17,
                FontAttributes = FontAttributes.Bold,
                FontFamily = "SegoeSemibold",
                CharacterSpacing = 0.3,
                VerticalOptions = LayoutOptions.Center,
            });
            Grid.SetColumn(headerLeft, 0);
            headerGrid.Children.Add(headerLeft);

            var calLabel = new Label
            {
                Text = $"{mealCalories:N0} kcal",
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb(colorHex),
                VerticalOptions = LayoutOptions.Center,
            };
            Grid.SetColumn(calLabel, 1);
            headerGrid.Children.Add(calLabel);

            sectionContent.Children.Add(headerGrid);

            // Divider
            sectionContent.Children.Add(new BoxView
            {
                HeightRequest = 1,
                Color = Application.Current?.RequestedTheme == AppTheme.Dark
                    ? Color.FromArgb("#2A2A42") : Color.FromArgb("#E5E7EB"),
            });

            // Items
            foreach (var record in items)
            {
                var itemGrid = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitionCollection
                    {
                        new ColumnDefinition(new GridLength(36)),
                        new ColumnDefinition(GridLength.Star),
                        new ColumnDefinition(GridLength.Auto),
                        new ColumnDefinition(new GridLength(32)),
                    },
                    ColumnSpacing = 10,
                    Padding = new Thickness(0, 4),
                };

                // Food icon (FluentUI)
                var foodIcon = new Label
                {
                    Text = record.FoodIcon,
                    FontFamily = AppIcons.FontFamily,
                    FontSize = 22,
                    TextColor = Color.FromArgb(colorHex),
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                };
                Grid.SetColumn(foodIcon, 0);
                itemGrid.Children.Add(foodIcon);

                // Name + servings
                var nameStack = new VerticalStackLayout { Spacing = 1, VerticalOptions = LayoutOptions.Center };
                nameStack.Children.Add(new Label
                {
                    Text = record.FoodName,
                    FontSize = 14,
                    FontAttributes = FontAttributes.Bold,
                    LineBreakMode = LineBreakMode.TailTruncation,
                });
                nameStack.Children.Add(new Label
                {
                    Text = $"{record.ServingsLabel} • {record.TimeLabel}",
                    FontSize = 11,
                    TextColor = Application.Current?.RequestedTheme == AppTheme.Dark
                        ? Color.FromArgb("#9CA3AF") : Color.FromArgb("#6B7280"),
                });
                Grid.SetColumn(nameStack, 1);
                itemGrid.Children.Add(nameStack);

                // Calories
                var calItemLabel = new Label
                {
                    Text = record.CaloriesLabel,
                    FontSize = 13,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb(colorHex),
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.End,
                };
                Grid.SetColumn(calItemLabel, 2);
                itemGrid.Children.Add(calItemLabel);

                // Delete button (FluentUI)
                var deleteBtn = new Label
                {
                    Text = AppIcons.Delete,
                    FontFamily = AppIcons.FontFamily,
                    FontSize = 16,
                    TextColor = Color.FromArgb("#F44336"),
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                };
                var deleteTap = new TapGestureRecognizer();
                var recordId = record.Id;
                var recordName = record.FoodName;
                deleteTap.Tapped += async (s, e) =>
                {
                    bool confirm = await DisplayAlert("Remove", $"Remove '{recordName}' from today's record?", "Yes", "Cancel");
                    if (confirm)
                    {
                        DietRecordService.DeleteRecord(recordId);
                        RefreshUI();
                    }
                };
                deleteBtn.GestureRecognizers.Add(deleteTap);
                Grid.SetColumn(deleteBtn, 3);
                itemGrid.Children.Add(deleteBtn);

                sectionContent.Children.Add(itemGrid);
            }

            sectionBorder.Content = sectionContent;
            MealSectionsContainer.Children.Add(sectionBorder);
        }
    }
}
