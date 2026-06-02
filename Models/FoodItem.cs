namespace FoodDrinkApp.Models;

public class FoodItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Calories { get; set; }
    public double Protein { get; set; }
    public double Carbs { get; set; }
    public double Fat { get; set; }
    public string AllergyNote { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;

    public string ImagePath { get; set; } = string.Empty;
    public string LocationText { get; set; } = string.Empty;

    /// <summary>
    /// Whether a photo has been attached. Used by the detail page to toggle
    /// between full-bleed photo hero and emoji fallback.
    /// </summary>
    public bool HasImage => !string.IsNullOrWhiteSpace(ImagePath);

    public string CaloriesLabel => $"{Calories} kcal";
    public string MacroSummary => $"Protein: {Protein}g, Carbs: {Carbs}g, Fat: {Fat}g";
    public string AccessibleSummary => $"{Name}. {Category}. {Calories} kcal. {MacroSummary}. {AllergyNote}";

    /// <summary>
    /// Returns a color associated with the food category, used for badge display.
    /// </summary>
    public Color CategoryBadgeColor => Category?.ToLower() switch
    {
        "breakfast" => Color.FromArgb("#FF6B35"),
        "lunch"     => Color.FromArgb("#4CAF50"),
        "dinner"    => Color.FromArgb("#E91E63"),
        "drink"     => Color.FromArgb("#2196F3"),
        "snack"     => Color.FromArgb("#FF9800"),
        "dessert"   => Color.FromArgb("#9C27B0"),
        _           => Color.FromArgb("#607D8B"),
    };

    /// <summary>
    /// Returns the emoji icon for the category.
    /// </summary>
    public string CategoryIcon => Category?.ToLower() switch
    {
        "breakfast" => "🌅",
        "lunch"     => "🥗",
        "dinner"    => "🍽️",
        "drink"     => "🥤",
        "snack"     => "🍿",
        "dessert"   => "🍰",
        _           => "🍴",
    };

    /// <summary>
    /// Short nutrition label for card display.
    /// </summary>
    public string ShortNutritionLabel => $"🔥 {Calories} kcal  •  💪 {Protein}g protein";
}