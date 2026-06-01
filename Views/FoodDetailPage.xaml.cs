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

    // 👇 處理詳情頁刪除按鈕點擊的邏輯 👇
    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (Item != null)
        {
            bool confirm = await DisplayAlert("Confirm Delete", $"Are you sure you want to delete '{Item.Name}'?", "Yes", "Cancel");
            if (confirm)
            {
                try { Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(50)); } catch { }

                // 刪除後自動返回上一頁 (首頁)
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