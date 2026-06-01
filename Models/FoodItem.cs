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

    public string CaloriesLabel => $"{Calories} kcal";
    public string MacroSummary => $"Protein: {Protein}g, Carbs: {Carbs}g, Fat: {Fat}g";
    public string AccessibleSummary => $"{Name}. {Category}. {Calories} kcal. {MacroSummary}. {AllergyNote}";
}