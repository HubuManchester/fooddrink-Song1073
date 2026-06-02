using FoodDrinkApp.Models;
using FoodDrinkApp.Services;

namespace FoodDrinkApp.Views;

[QueryProperty(nameof(Item), "Item")]
public partial class FoodDetailPage : ContentPage
{
    private FoodItem? _item;
    public FoodItem? Item
    {
        get => _item;
        set { _item = value; OnPropertyChanged(); }
    }

    public FoodDetailPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        BindingContext = Item;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
    }

    private async void OnBackTapped(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OnSpeakClicked(object? sender, EventArgs e)
    {
        if (Item != null)
        {
            await SpeechService.SpeakTextAsync(Item.AccessibleSummary);
        }
    }

    private void OnStopSpeakClicked(object? sender, EventArgs e)
    {
        SpeechService.Stop();
    }

    // ── Add this food to today's diet record ──
    private async void OnAddToDietClicked(object? sender, EventArgs e)
    {
        if (Item == null) return;

        string? mealType = await DisplayActionSheet("Select Meal", "Cancel", null,
            "Breakfast", "Lunch", "Dinner", "Snack");
        if (string.IsNullOrEmpty(mealType) || mealType == "Cancel") return;

        string? servingsStr = await DisplayPromptAsync("Servings",
            $"How many servings of {Item.Name}?",
            accept: "Add", cancel: "Cancel",
            placeholder: "Enter servings", maxLength: 5,
            keyboard: Keyboard.Numeric,
            initialValue: "1");

        if (string.IsNullOrWhiteSpace(servingsStr)) return;
        if (!double.TryParse(servingsStr, out double servings) || servings <= 0)
        {
            await DisplayAlert("Invalid", "Servings must be a positive number.", "OK");
            return;
        }

        var record = DietRecord.FromFoodItem(Item, mealType, servings);
        DietRecordService.AddRecord(record);

        try { HapticFeedback.Default.Perform(HapticFeedbackType.Click); } catch { }
        await DisplayAlert("Added!",
            $"'{Item.Name}' × {servings:N1} added to {mealType}.\nTotal: {record.TotalCalories:N0} kcal",
            "OK");
    }

    // ── Handle detail page delete button click ──
    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (Item != null)
        {
            bool confirm = await DisplayAlert("Confirm Delete", $"Are you sure you want to delete '{Item.Name}'?", "Yes", "Cancel");
            if (confirm)
            {
                try { Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(50)); } catch { }

                // Automatically navigate back to the previous page (Home) after deletion
                await FoodCatalogService.DeleteFoodAsync(Item.Id);
                await Shell.Current.GoToAsync("..");
            }
        }
    }

    protected override void OnDisappearing()
    {
        SpeechService.Stop();
        base.OnDisappearing();
    }
}