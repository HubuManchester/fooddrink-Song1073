using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodDrinkApp.Models;
using FoodDrinkApp.Services;

namespace FoodDrinkApp.ViewModels;

public partial class MainViewModel : BaseViewModel
{
    private List<FoodItem> _allFoods = new();

    [ObservableProperty]
    private ObservableCollection<FoodCategoryGroup> _categoryGroups = new();

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string _totalCountText = "Loading...";

    [ObservableProperty]
    private string _totalCaloriesText = string.Empty;

    [ObservableProperty]
    private bool _isRefreshing;

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        IsRefreshing = true;

        try
        {
            _allFoods = await FoodCatalogService.GetFoodsAsync();
            ApplySearchFilter(SearchQuery);
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private void Search()
    {
        ApplySearchFilter(SearchQuery);
    }

    partial void OnSearchQueryChanged(string value)
    {
        ApplySearchFilter(value);
    }

    private void ApplySearchFilter(string? query)
    {
        List<FoodItem> displayedFoods;

        if (string.IsNullOrWhiteSpace(query))
        {
            displayedFoods = _allFoods.ToList();
        }
        else
        {
            query = query.ToLower();
            displayedFoods = _allFoods.Where(f =>
                (f.Name != null && f.Name.ToLower().Contains(query)) ||
                (f.Category != null && f.Category.ToLower().Contains(query)) ||
                (f.Tags != null && f.Tags.ToLower().Contains(query))
            ).ToList();
        }

        // Update summary card with today's diet progress
        var profile = ProfileService.GetActiveProfile();
        double consumed = DietRecordService.GetTodayTotalCalories();

        if (profile != null)
        {
            ProfileService.RefreshDailyCalories(profile);
            double remaining = profile.DailyCalorieTarget - consumed;
            TotalCountText = $"{displayedFoods.Count} foods available  ·  {profile.Name}";
            TotalCaloriesText = remaining >= 0
                ? $"{consumed:N0} / {profile.DailyCalorieTarget:N0} kcal  ·  {remaining:N0} remaining"
                : $"{consumed:N0} / {profile.DailyCalorieTarget:N0} kcal  ·  {Math.Abs(remaining):N0} over!";
        }
        else
        {
            TotalCountText = $"{displayedFoods.Count} items in your collection";
            TotalCaloriesText = "Set up a profile to track daily calories";
        }

        // Build category sections
        BuildCategorySections(displayedFoods);
    }

    private void BuildCategorySections(List<FoodItem> foods)
    {
        var grouped = foods.GroupBy(f => f.Category)
                           .OrderBy(g => FoodCatalogService.CategoryOrder(g.Key));

        var newGroups = new ObservableCollection<FoodCategoryGroup>();
        foreach (var group in grouped)
        {
            newGroups.Add(new FoodCategoryGroup
            {
                CategoryName = group.Key,
                Subtitle = FoodCategoryGroup.GetSubtitleForCategory(group.Key),
                Items = group.ToList()
            });
        }
        CategoryGroups = newGroups;
    }

    [RelayCommand]
    private async Task AddNewItemAsync()
    {
        await Shell.Current.GoToAsync("AddItemPage");
    }

    [RelayCommand]
    private async Task GoToDetailAsync(FoodItem food)
    {
        if (food == null) return;
        var navigationParameter = new Dictionary<string, object> { { "Item", food } };
        await Shell.Current.GoToAsync("FoodDetailPage", navigationParameter);
    }
}
