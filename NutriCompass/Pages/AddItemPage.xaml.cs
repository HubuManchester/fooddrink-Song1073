using System;
using System.Globalization;
using Microsoft.Maui.Devices.Sensors;
namespace NutriCompass.Pages;

public partial class AddItemPage : AccessibleContentPage
{
    public AddItemPage()
    {
        InitializeComponent();
    }

    private void OnSaveItemClicked(object? sender, EventArgs e)
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

        DisplayAlert("Saved", "Food item recorded", "OK");
        ClearForm();
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
