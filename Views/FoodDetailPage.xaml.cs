using FoodDrinkApp.Services;

namespace FoodDrinkApp.Views;

public partial class FoodDetailPage : ContentPage
{
    public FoodDetailPage()
    {
        InitializeComponent();
        BindingContext = new ViewModels.FoodDetailViewModel();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
    }

    protected override void OnDisappearing()
    {
        SpeechService.Stop();
        base.OnDisappearing();
    }
}