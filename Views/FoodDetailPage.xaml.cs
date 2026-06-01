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

    protected override void OnDisappearing()
    {
        SpeechService.Stop();
        base.OnDisappearing();
    }
}