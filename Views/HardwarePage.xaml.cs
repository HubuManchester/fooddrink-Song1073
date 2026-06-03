using FoodDrinkApp.Services;

namespace FoodDrinkApp.Views;

public partial class HardwarePage : ContentPage
{
    public HardwarePage()
    {
        InitializeComponent();
        BindingContext = new ViewModels.HardwareViewModel();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
    }
}