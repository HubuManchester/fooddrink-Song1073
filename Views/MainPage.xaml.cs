using FoodDrinkApp.Models;
using FoodDrinkApp.Services;

namespace FoodDrinkApp.Views;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
        await LoadDataAsync();
    }

    private async Task LoadDataAsync(string query = "")
    {
        FoodRefreshView.IsRefreshing = true;
        var foods = await FoodCatalogService.GetFoodsAsync(query);
        FoodCollection.ItemsSource = foods;
        FoodRefreshView.IsRefreshing = false;
    }

    private async void OnRefreshing(object? sender, EventArgs e)
    {
        FoodSearchBar.Text = string.Empty;
        await LoadDataAsync();
    }

    private async void OnSearchPressed(object? sender, EventArgs e)
    {
        var query = FoodSearchBar.Text ?? string.Empty;
        await LoadDataAsync(query);
    }

    private async void OnAddClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("AddItemPage");
    }

    private async void OnFoodSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is FoodItem selectedFood)
        {
            var navigationParameter = new Dictionary<string, object> { { "Item", selectedFood } };
            await Shell.Current.GoToAsync("FoodDetailPage", navigationParameter);
            FoodCollection.SelectedItem = null;
        }
    }
}