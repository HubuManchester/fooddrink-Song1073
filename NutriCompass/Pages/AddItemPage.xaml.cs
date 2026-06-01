using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Maui.Devices.Sensors;
using NutriCompass.Models;
using NutriCompass.Services;

namespace NutriCompass.Pages;

public partial class AddItemPage : AccessibleContentPage
{
    private readonly FoodCatalogService _catalogService;

    public AddItemPage(FoodCatalogService catalogService)
    {
        InitializeComponent();
        _catalogService = catalogService;
    }

    private async void OnSaveItemClicked(object? sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;
        var errors = ValidateForm(out var calories);
        if (errors.Length > 0)
        {
            ErrorLabel.Text = string.Join("\n", errors);
            ErrorLabel.IsVisible = true;
            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(200));
            return;
        }

        var newItem = new FoodItem
        {
            Name = NameEntry.Text?.Trim() ?? string.Empty,
            Category = CategoryEntry.Text?.Trim() ?? string.Empty,
            Description = DescriptionEntry.Text?.Trim() ?? string.Empty,
            MacroSummary = $"{calories:N0} kcal",
            Tags = string.Empty
        };

        var result = await _catalogService.SubmitFoodAsync(newItem);
        var title = result.IsSuccess ? "Saved" : "Offline";
        await DisplayAlert(title, result.Message, "OK");

        if (result.IsSuccess)
        {
            ClearForm();
        }
    }

    private string[] ValidateForm(out double calorieValue)
    {
        var list = new System.Collections.Generic.List<string>();
        calorieValue = 0;

        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            list.Add("Name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(CategoryEntry.Text))
        {
            list.Add("Category cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(DescriptionEntry.Text))
        {
            list.Add("Description cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(CaloriesEntry.Text) ||
            !double.TryParse(CaloriesEntry.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) ||
            parsed < 0)
        {
            list.Add("Calories must be a non-negative number.");
        }
        else
        {
            calorieValue = parsed;
        }

        return list.ToArray();
    }

    private void ClearForm()
    {
        NameEntry.Text = string.Empty;
        CategoryEntry.Text = string.Empty;
        DescriptionEntry.Text = string.Empty;
        CaloriesEntry.Text = string.Empty;
    }
}
