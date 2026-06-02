namespace FoodDrinkApp.Models;

/// <summary>
/// Represents a user profile with personal information used to calculate
/// Total Daily Energy Expenditure (TDEE) via the Mifflin-St Jeor equation.
/// </summary>
public class UserProfile
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;

    /// <summary>Local file path to the profile avatar image (empty = use default icon)</summary>
    public string ProfileImagePath { get; set; } = string.Empty;

    /// <summary>Gender: "Male" or "Female"</summary>
    public string Gender { get; set; } = "Male";

    /// <summary>Age in years</summary>
    public int Age { get; set; } = 25;

    /// <summary>Height in centimetres</summary>
    public double HeightCm { get; set; } = 170;

    /// <summary>Weight in kilograms</summary>
    public double WeightKg { get; set; } = 70;

    /// <summary>
    /// Activity level multiplier label.
    /// Sedentary / Lightly Active / Moderately Active / Very Active / Extra Active
    /// </summary>
    public string ActivityLevel { get; set; } = "Moderately Active";

    /// <summary>
    /// Goal: "Lose Weight", "Maintain", or "Gain Weight"
    /// </summary>
    public string Goal { get; set; } = "Maintain";

    /// <summary>Location text obtained via geocoding (e.g. "Manchester, United Kingdom")</summary>
    public string LocationText { get; set; } = string.Empty;

    /// <summary>Latitude for climate-based adjustment</summary>
    public double Latitude { get; set; }

    /// <summary>Longitude for climate-based adjustment</summary>
    public double Longitude { get; set; }

    /// <summary>The date this profile's daily calorie was last calculated</summary>
    public DateTime LastCalculatedDate { get; set; } = DateTime.MinValue;

    /// <summary>Cached daily calorie target (recalculated daily)</summary>
    public int DailyCalorieTarget { get; set; }

    /// <summary>Whether this profile is the currently active one</summary>
    public bool IsActive { get; set; }

    // ─── Computed helpers ───

    /// <summary>
    /// Returns the activity level multiplier for the TDEE formula.
    /// </summary>
    public double ActivityMultiplier => ActivityLevel switch
    {
        "Sedentary"        => 1.2,
        "Lightly Active"   => 1.375,
        "Moderately Active" => 1.55,
        "Very Active"      => 1.725,
        "Extra Active"     => 1.9,
        _                  => 1.55,
    };

    /// <summary>
    /// Goal-based calorie adjustment (kcal offset from TDEE).
    /// </summary>
    public int GoalCalorieOffset => Goal switch
    {
        "Lose Weight" => -500,
        "Gain Weight" => +300,
        _             => 0,
    };

    /// <summary>
    /// Calculates BMR using the Mifflin-St Jeor equation.
    /// </summary>
    public double CalculateBMR()
    {
        if (Gender.Equals("Male", StringComparison.OrdinalIgnoreCase))
            return (10 * WeightKg) + (6.25 * HeightCm) - (5 * Age) + 5;
        else
            return (10 * WeightKg) + (6.25 * HeightCm) - (5 * Age) - 161;
    }

    /// <summary>
    /// Calculates the recommended daily calorie intake, considering activity level,
    /// goal, and a small climate-based adjustment.
    /// </summary>
    public int CalculateDailyCalories()
    {
        double bmr = CalculateBMR();
        double tdee = bmr * ActivityMultiplier;

        // Climate adjustment: colder regions (high latitude) +50-100 kcal
        double climateBonus = 0;
        if (Math.Abs(Latitude) > 50) climateBonus = 100;
        else if (Math.Abs(Latitude) > 35) climateBonus = 50;

        int total = (int)Math.Round(tdee + GoalCalorieOffset + climateBonus);
        return Math.Max(total, 1200); // safety floor
    }

    /// <summary>
    /// Display-friendly summary of macros breakdown based on calorie target.
    /// </summary>
    public string MacroBreakdown
    {
        get
        {
            int cal = DailyCalorieTarget > 0 ? DailyCalorieTarget : CalculateDailyCalories();
            int protein = (int)(cal * 0.30 / 4); // 30% protein
            int carbs = (int)(cal * 0.45 / 4);   // 45% carbs
            int fat = (int)(cal * 0.25 / 9);      // 25% fat
            return $"P: {protein}g • C: {carbs}g • F: {fat}g";
        }
    }

    public string DisplayLabel => $"{Name} ({Gender}, {Age}y, {WeightKg}kg)";
    public string CalorieLabel => $"{DailyCalorieTarget} kcal/day";
    /// <summary>
    /// Returns a Fluent UI icon glyph for the goal.
    /// Used with FontFamily="FluentIcons" in code-behind.
    /// </summary>
    public string GoalIcon => Goal switch
    {
        "Lose Weight" => "\ue11a",  // arrow_trending_down_20
        "Gain Weight" => "\uf196",  // arrow_trending_16 (up)
        _ => "\ue11c",              // arrow_trending_lines_20 (balance)
    };
}
