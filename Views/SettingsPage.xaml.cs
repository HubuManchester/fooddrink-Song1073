using FoodDrinkApp.Services;

namespace FoodDrinkApp.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
        BindingContext = new ViewModels.SettingsViewModel();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);

        if (BindingContext is ViewModels.SettingsViewModel vm)
        {
            vm.Initialize();
        }
    }
}