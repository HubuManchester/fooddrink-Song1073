namespace FoodDrinkApp.Models;

/// <summary>
/// Represents a single food intake entry for a specific date.
/// Each record links a FoodItem to a meal type and timestamp.
/// </summary>
public class DietRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>The profile this record belongs to (empty = legacy/unscoped)</summary>
    public string ProfileId { get; set; } = string.Empty;

    /// <summary>The date this record belongs to (date only, no time)</summary>
    public DateTime Date { get; set; } = DateTime.Today;

    /// <summary>Meal type: Breakfast / Lunch / Dinner / Snack</summary>
    public string MealType { get; set; } = string.Empty;

    /// <summary>Time the food was consumed</summary>
    public DateTime ConsumedAt { get; set; } = DateTime.Now;

    /// <summary>Number of servings</summary>
    public double Servings { get; set; } = 1.0;

    // ─── Food item snapshot (denormalised for offline safety) ───

    public string FoodName { get; set; } = string.Empty;
    public string FoodCategory { get; set; } = string.Empty;
    public double Calories { get; set; }
    public double Protein { get; set; }
    public double Carbs { get; set; }
    public double Fat { get; set; }
    public string FoodIcon { get; set; } = "\uf1ec";

    // ─── Computed ───

    public double TotalCalories => Calories * Servings;
    public double TotalProtein => Protein * Servings;
    public double TotalCarbs => Carbs * Servings;
    public double TotalFat => Fat * Servings;

    public string TimeLabel => ConsumedAt.ToString("HH:mm");
    public string CaloriesLabel => $"{TotalCalories:N0} kcal";
    public string ServingsLabel => Servings == 1.0 ? "1 serving" : $"{Servings:N1} servings";
    public string SummaryLabel => $"{FoodName} · {CaloriesLabel}";

    /// <summary>
    /// Creates a DietRecord from a FoodItem, automatically capturing the active profile.
    /// </summary>
    public static DietRecord FromFoodItem(FoodItem food, string mealType, double servings = 1.0)
    {
        var activeProfile = FoodDrinkApp.Services.ProfileService.GetActiveProfile();
        return new DietRecord
        {
            ProfileId = activeProfile?.Id ?? string.Empty,
            MealType = mealType,
            Servings = servings,
            FoodName = food.Name,
            FoodCategory = food.Category,
            Calories = food.Calories,
            Protein = food.Protein,
            Carbs = food.Carbs,
            Fat = food.Fat,
            FoodIcon = food.CategoryIcon,
            ConsumedAt = DateTime.Now,
            Date = DateTime.Today,
        };
    }
}
