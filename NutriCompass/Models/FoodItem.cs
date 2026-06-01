namespace NutriCompass.Models;

public sealed class FoodItem
{
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string MacroSummary { get; init; } = string.Empty;
    public string Tags { get; init; } = string.Empty;
}
