using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodDrinkApp.Models;
using FoodDrinkApp.Services;
using Microsoft.Maui.Devices;

namespace FoodDrinkApp.ViewModels;

public partial class MealGroup : ObservableObject
{
    [ObservableProperty]
    private string _mealName = string.Empty;

    [ObservableProperty]
    private string _icon = string.Empty;

    [ObservableProperty]
    private Color _color = Colors.Gray;

    [ObservableProperty]
    private string _caloriesText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<DietRecord> _items = new();
}

public partial class DietRecordViewModel : BaseViewModel
{
    [ObservableProperty]
    private string _dateText = string.Empty;

    [ObservableProperty]
    private string _consumedText = "0";

    [ObservableProperty]
    private string _targetText = "—";

    [ObservableProperty]
    private string _todayProteinText = "0g";

    [ObservableProperty]
    private string _todayCarbsText = "0g";

    [ObservableProperty]
    private string _remainingText = "—";

    [ObservableProperty]
    private Color _remainingColor = Colors.Green;

    [ObservableProperty]
    private double _progressRatio;

    [ObservableProperty]
    private double _progressBarWidth;

    [ObservableProperty]
    private Color _progressColor = Colors.Green;

    [ObservableProperty]
    private bool _isEmpty;

    [ObservableProperty]
    private ObservableCollection<MealGroup> _mealSections = new();

    [ObservableProperty]
    private bool _isRefreshing;

    [RelayCommand]
    public void Refresh()
    {
        IsRefreshing = true;

        DateText = DateTime.Today.ToString("dddd, MMMM dd, yyyy");

        var profile = ProfileService.GetActiveProfile();
        double consumed = DietRecordService.GetTodayTotalCalories();
        var (protein, carbs, fat) = DietRecordService.GetTodayMacros();

        int target = 0;
        if (profile != null)
        {
            ProfileService.RefreshDailyCalories(profile);
            target = profile.DailyCalorieTarget;
        }

        ConsumedText = consumed.ToString("N0");
        TargetText = target > 0 ? $"{target:N0} kcal" : "Set up a Profile first";
        TodayProteinText = $"{protein:N0}g";
        TodayCarbsText = $"{carbs:N0}g";

        double remaining = target - consumed;
        RemainingText = target > 0 ? $"{remaining:N0}" : "—";
        RemainingColor = remaining >= 0 ? Color.FromArgb("#4CAF50") : Color.FromArgb("#F44336");

        if (target > 0)
        {
            ProgressRatio = Math.Clamp(consumed / target, 0, 1.0);
            ProgressColor = remaining >= 0 ? Color.FromArgb("#4CAF50") : Color.FromArgb("#F44336");
        }
        else
        {
            ProgressRatio = 0;
            ProgressColor = Color.FromArgb("#4CAF50");
        }

        var parentWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density - 80;
        if (parentWidth < 100) parentWidth = 300;
        ProgressBarWidth = parentWidth * ProgressRatio;

        BuildMealSections();

        IsRefreshing = false;
    }

    private void BuildMealSections()
    {
        var grouped = DietRecordService.GetTodayGroupedByMeal();
        IsEmpty = grouped.Count == 0;

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

        var newSections = new ObservableCollection<MealGroup>();

        foreach (var meal in grouped)
        {
            var mealName = meal.Key;
            var items = meal.Value.ToList();
            var icon = mealIcons.GetValueOrDefault(mealName, "\uf1ec");
            var colorHex = mealColors.GetValueOrDefault(mealName, "#607D8B");
            var mealCalories = items.Sum(r => r.TotalCalories);

            newSections.Add(new MealGroup
            {
                MealName = mealName,
                Icon = icon,
                Color = Color.FromArgb(colorHex),
                CaloriesText = $"{mealCalories:N0} kcal",
                Items = new ObservableCollection<DietRecord>(items)
            });
        }

        MealSections = newSections;
    }

    [RelayCommand]
    private async Task AddFoodAsync()
    {
        if (Shell.Current?.CurrentPage == null) return;
        var page = Shell.Current.CurrentPage;

        string? mealType = await page.DisplayActionSheet("Which meal?", "Cancel", null, "Breakfast", "Lunch", "Dinner", "Snack");
        if (string.IsNullOrEmpty(mealType) || mealType == "Cancel") return;

        var allFoods = await FoodCatalogService.GetFoodsAsync();
        if (allFoods.Count == 0)
        {
            await page.DisplayAlert("No Foods", "No food items available. Add some from the Home page first.", "OK");
            return;
        }

        var foodNames = allFoods.Select(f => $"{f.Name} ({f.Category} · {f.Calories} kcal)").ToArray();
        string? selectedFoodName = await page.DisplayActionSheet("Select a food", "Cancel", null, foodNames);
        if (string.IsNullOrEmpty(selectedFoodName) || selectedFoodName == "Cancel") return;

        int selectedIndex = Array.IndexOf(foodNames, selectedFoodName);
        if (selectedIndex < 0 || selectedIndex >= allFoods.Count) return;
        var selectedFood = allFoods[selectedIndex];

        string? servingsStr = await page.DisplayPromptAsync("Servings", $"How many servings of {selectedFood.Name}?", accept: "Add", cancel: "Cancel", placeholder: "1", maxLength: 5, keyboard: Keyboard.Numeric, initialValue: "1");

        if (string.IsNullOrWhiteSpace(servingsStr)) return;
        if (!double.TryParse(servingsStr, out double servings) || servings <= 0)
        {
            await page.DisplayAlert("Invalid", "Servings must be a positive number.", "OK");
            return;
        }

        var record = DietRecord.FromFoodItem(selectedFood, mealType, servings);
        DietRecordService.AddRecord(record);

        try { HapticFeedback.Default.Perform(HapticFeedbackType.Click); } catch { }

        Refresh();

        await page.DisplayAlert("Added!", $"'{selectedFood.Name}' × {servings:N1}\nAdded to {mealType}\nTotal: {record.TotalCalories:N0} kcal", "OK");
    }

    [RelayCommand]
    private async Task DeleteRecordAsync(DietRecord record)
    {
        if (record == null || Shell.Current?.CurrentPage == null) return;
        
        bool confirm = await Shell.Current.CurrentPage.DisplayAlert("Remove", $"Remove '{record.FoodName}' from today's record?", "Yes", "Cancel");
        if (confirm)
        {
            DietRecordService.DeleteRecord(record.Id);
            Refresh();
        }
    }
}
