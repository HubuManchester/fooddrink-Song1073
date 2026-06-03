namespace FoodDrinkApp.Models;

public class FoodItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Food type category (e.g. Grain, Meat, Vegetable, Fruit, Dairy, Seafood, Beverage, Dessert).
    /// NOT meal time — meal time (Breakfast/Lunch/Dinner/Snack) is a label on DietRecord only.
    /// </summary>
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

    /// <summary>
    /// Returns the correct ImageSource for this item.
    /// Handles both web URLs (http/https) and local file paths.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public ImageSource? ResolvedImageSource
    {
        get
        {
            if (!HasImage) return null;
            if (ImagePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                ImagePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return ImageSource.FromUri(new Uri(ImagePath));
            return ImageSource.FromFile(ImagePath);
        }
    }

    public string CaloriesLabel => $"{Calories} kcal";
    public string MacroSummary => $"Protein: {Protein}g, Carbs: {Carbs}g, Fat: {Fat}g";
    public string AccessibleSummary => $"{Name}. {Category}. {Calories} kcal. {MacroSummary}. {AllergyNote}";

    /// <summary>
    /// Returns a color associated with the food type category, used for badge display.
    /// </summary>
    public Color CategoryBadgeColor => Category?.ToLower() switch
    {
        "grain"     => Color.FromArgb("#FF8F00"),  // amber
        "meat"      => Color.FromArgb("#D32F2F"),  // red
        "vegetable" => Color.FromArgb("#388E3C"),  // green
        "fruit"     => Color.FromArgb("#F57C00"),  // orange
        "dairy"     => Color.FromArgb("#1976D2"),  // blue
        "seafood"   => Color.FromArgb("#00838F"),  // teal
        "beverage"  => Color.FromArgb("#7B1FA2"),  // purple
        "dessert"   => Color.FromArgb("#C2185B"),  // pink
        _           => Color.FromArgb("#607D8B"),  // grey
    };

    /// <summary>
    /// Returns the category color with 15% opacity for background tinting.
    /// </summary>
    public Color CategoryBadgeBgColor => CategoryBadgeColor.WithAlpha(0.15f);

    /// <summary>
    /// Returns the Fluent UI icon glyph for the food type category.
    /// Used with FontFamily="FluentIcons" in code-behind.
    /// </summary>
    public string CategoryIcon => Category?.ToLower() switch
    {
        "grain"     => "\uf1ec",  // bowl_chopsticks_24
        "meat"      => "\uf1ec",  // bowl_chopsticks_24
        "vegetable" => "\ueee8",  // bowl_salad_20
        "fruit"     => "\ueee8",  // bowl_salad_20
        "dairy"     => "\uf1ec",  // bowl_chopsticks_24
        "seafood"   => "\uf1ec",  // bowl_chopsticks_24
        "beverage"  => "\uf1ec",  // bowl_chopsticks_24
        "dessert"   => "\uf1ec",  // bowl_chopsticks_24
        _           => "\uf1ec",  // bowl_chopsticks_24
    };

    /// <summary>
    /// Short nutrition label for card display.
    /// </summary>
    public string ShortNutritionLabel => $"{Calories} kcal  ·  {Protein}g protein";
}