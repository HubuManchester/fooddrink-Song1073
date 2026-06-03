using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Shapes;
using FoodDrinkApp.Models;
using FoodDrinkApp.Services;
using FoodDrinkApp.Helpers;

namespace FoodDrinkApp.Views;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        BindingContext = new ViewModels.MainViewModel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
        
        if (BindingContext is ViewModels.MainViewModel vm)
        {
            await vm.LoadDataCommand.ExecuteAsync(null);
        }
    }
}