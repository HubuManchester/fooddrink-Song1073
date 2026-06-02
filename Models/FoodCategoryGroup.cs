namespace FoodDrinkApp.Models;

/// <summary>
/// Represents a category group for display in the main page's section-based layout.
/// Each group has a title, subtitle, and a list of food items in that category.
/// </summary>
public class FoodCategoryGroup
{
    public string CategoryName { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public List<FoodItem> Items { get; set; } = new();

    /// <summary>
    /// Provides a contextual subtitle based on category.
    /// </summary>
    public static string GetSubtitleForCategory(string category) => category?.ToLower() switch
    {
        "breakfast" => "Start your day right",
        "lunch"     => "Midday fuel",
        "dinner"    => "Evening favourites",
        "drink"     => "Stay refreshed",
        "snack"     => "Quick bites",
        "dessert"   => "Sweet treats",
        _           => "Explore more",
    };
}
