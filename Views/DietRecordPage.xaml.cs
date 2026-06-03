using FoodDrinkApp.Services;

namespace FoodDrinkApp.Views;

public partial class DietRecordPage : ContentPage
{
    public DietRecordPage()
    {
        InitializeComponent();
        BindingContext = new ViewModels.DietRecordViewModel();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
        if (BindingContext is ViewModels.DietRecordViewModel vm)
        {
            vm.RefreshCommand.Execute(null);
        }
    }
}
