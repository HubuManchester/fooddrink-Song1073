namespace FoodDrinkApp.Models;

/// <summary>
/// Represents a category group for display in the main page's section-based layout.
/// Each group has a title, subtitle, and a list of food items in that category.
/// Categories are food types (Grain, Meat, Vegetable, etc.), NOT meal times.
/// </summary>
public class FoodCategoryGroup
{
    public string CategoryName { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public List<FoodItem> Items { get; set; } = new();

    /// <summary>
    /// Provides a contextual subtitle based on food type category.
    /// </summary>
    public static string GetSubtitleForCategory(string category) => category?.ToLower() switch
    {
        "grain"     => "Breads, rice & cereals",
        "meat"      => "Poultry, beef & pork",
        "vegetable" => "Fresh greens & veggies",
        "fruit"     => "Nature's sweet treats",
        "dairy"     => "Milk, cheese & yogurt",
        "seafood"   => "Fish & shellfish",
        "beverage"  => "Drinks & smoothies",
        "dessert"   => "Sweet indulgences",
        _           => "Explore more",
    };
}
