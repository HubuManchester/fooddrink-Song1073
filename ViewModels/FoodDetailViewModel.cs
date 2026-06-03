using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodDrinkApp.Models;
using FoodDrinkApp.Services;

namespace FoodDrinkApp.ViewModels;

[QueryProperty(nameof(Item), "Item")]
public partial class FoodDetailViewModel : BaseViewModel
{
    [ObservableProperty]
    private FoodItem? _item;

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task SpeakAsync()
    {
        if (Item != null)
        {
            await SpeechService.SpeakTextAsync(Item.AccessibleSummary);
        }
    }

    [RelayCommand]
    private void StopSpeak()
    {
        SpeechService.Stop();
    }

    [RelayCommand]
    private async Task AddToDietAsync()
    {
        if (Item == null || Shell.Current?.CurrentPage == null) return;

        var page = Shell.Current.CurrentPage;

        string? mealType = await page.DisplayActionSheet("Select Meal", "Cancel", null,
            "Breakfast", "Lunch", "Dinner", "Snack");
        if (string.IsNullOrEmpty(mealType) || mealType == "Cancel") return;

        string? servingsStr = await page.DisplayPromptAsync("Servings",
            $"How many servings of {Item.Name}?",
            accept: "Add", cancel: "Cancel",
            placeholder: "Enter servings", maxLength: 5,
            keyboard: Keyboard.Numeric,
            initialValue: "1");

        if (string.IsNullOrWhiteSpace(servingsStr)) return;
        if (!double.TryParse(servingsStr, out double servings) || servings <= 0)
        {
            await page.DisplayAlert("Invalid", "Servings must be a positive number.", "OK");
            return;
        }

        var record = DietRecord.FromFoodItem(Item, mealType, servings);
        DietRecordService.AddRecord(record);

        try { HapticFeedback.Default.Perform(HapticFeedbackType.Click); } catch { }
        await page.DisplayAlert("Added!",
            $"'{Item.Name}' × {servings:N1} added to {mealType}.\nTotal: {record.TotalCalories:N0} kcal",
            "OK");
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (Item == null || Shell.Current?.CurrentPage == null) return;
        
        var page = Shell.Current.CurrentPage;

        bool confirm = await page.DisplayAlert("Confirm Delete", $"Are you sure you want to delete '{Item.Name}'?", "Yes", "Cancel");
        if (confirm)
        {
            try { Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(50)); } catch { }

            await FoodCatalogService.DeleteFoodAsync(Item.Id);
            await Shell.Current.GoToAsync("..");
        }
    }
}
