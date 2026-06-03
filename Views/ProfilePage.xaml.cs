using FoodDrinkApp.Services;

namespace FoodDrinkApp.Views;

public partial class ProfilePage : ContentPage
{
    public ProfilePage()
    {
        InitializeComponent();
        BindingContext = new ViewModels.ProfileViewModel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
        if (BindingContext is ViewModels.ProfileViewModel vm)
        {
            await vm.InitializeAsync();
        }
    }
}
