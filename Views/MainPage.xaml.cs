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

    // 👇 處理滑動刪除按鈕點擊的邏輯 👇
    private async void OnDeleteInvoked(object? sender, EventArgs e)
    {
        if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is FoodItem itemToDelete)
        {
            // 彈出確認對話框
            bool confirm = await DisplayAlert("Confirm Delete", $"Are you sure you want to delete '{itemToDelete.Name}'?", "Yes", "Cancel");
            if (confirm)
            {
                // 刪除時觸發輕微震動回饋 (結合硬體功能！)
                try { Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(50)); } catch { }

                // 呼叫 Service 刪除並重新載入列表
                await FoodCatalogService.DeleteFoodAsync(itemToDelete.Id);
                await LoadDataAsync(FoodSearchBar.Text);
            }
        }
    }
}