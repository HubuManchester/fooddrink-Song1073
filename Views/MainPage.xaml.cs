using System.Collections.ObjectModel;
using FoodDrinkApp.Models;
using FoodDrinkApp.Services;

namespace FoodDrinkApp.Views;

public partial class MainPage : ContentPage
{
    private ObservableCollection<FoodItem> _displayFoods = new ObservableCollection<FoodItem>();

    public MainPage()
    {
        InitializeComponent();
        FoodCollection.ItemsSource = _displayFoods;
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

        _displayFoods.Clear();
        foreach (var item in foods)
        {
            _displayFoods.Add(item);
        }

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

    private async void OnDeleteInvoked(object? sender, EventArgs e)
    {
        if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is FoodItem itemToDelete)
        {
            bool confirm = await DisplayAlert("Confirm Delete", $"Are you sure you want to delete '{itemToDelete.Name}'?", "Yes", "Cancel");
            if (confirm)
            {
                try { Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(50)); } catch { }

                await FoodCatalogService.DeleteFoodAsync(itemToDelete.Id);

                var itemToRemove = _displayFoods.FirstOrDefault(f => f.Id == itemToDelete.Id);
                if (itemToRemove != null)
                {
                    _displayFoods.Remove(itemToRemove);
                }
            }
        }
    }
}