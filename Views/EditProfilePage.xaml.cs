using FoodDrinkApp.Services;

namespace FoodDrinkApp.Views;

public partial class EditProfilePage : ContentPage
{
    public EditProfilePage()
    {
        InitializeComponent();
        BindingContext = new ViewModels.EditProfileViewModel();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
    }
}
