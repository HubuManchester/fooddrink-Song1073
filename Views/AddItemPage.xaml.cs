using FoodDrinkApp.Services;

namespace FoodDrinkApp.Views;

public partial class AddItemPage : ContentPage
{
    public AddItemPage()
    {
        InitializeComponent();
        BindingContext = new ViewModels.AddItemViewModel();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
    }
}