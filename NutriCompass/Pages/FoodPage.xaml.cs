using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using NutriCompass.Models;
using NutriCompass.Services;

namespace NutriCompass.Pages;

public partial class FoodPage : AccessibleContentPage
{
    private readonly FoodCatalogService _catalogService;
    private readonly List<FoodItem> _backingList = new();

    public ObservableCollection<FoodItem> FoodItems { get; } = new();

    public FoodPage(FoodCatalogService catalogService)
    {
        InitializeComponent();
        BindingContext = this;
        _catalogService = catalogService;
        LoadCatalogAsync();
    }

    private async void LoadCatalogAsync()
    {
        try
        {
            var result = await _catalogService.GetFoodsAsync();
            _backingList.Clear();
            _backingList.AddRange(result.Items);
            RefreshItems(_backingList);

            if (result.UsedFallback)
            {
                await DisplayAlert("Offline", "Unable to load remote catalog. Showing local favorites.", "OK");
            }
        }
        catch
        {
            await DisplayAlert("Offline", "Unable to load remote catalog right now.", "OK");
        }
    }

    private void RefreshItems(IEnumerable<FoodItem> items)
    {
        FoodItems.Clear();
        foreach (var item in items)
        {
            FoodItems.Add(item);
        }
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        var query = (e?.NewTextValue ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(query))
        {
            RefreshItems(_backingList);
            return;
        }

        var lower = query.ToLowerInvariant();
        var filtered = _backingList.Where(item =>
            item.Name.Contains(lower, StringComparison.OrdinalIgnoreCase) ||
            item.Category.Contains(lower, StringComparison.OrdinalIgnoreCase) ||
            item.Tags.Contains(lower, StringComparison.OrdinalIgnoreCase));

        RefreshItems(filtered);
    }
}
