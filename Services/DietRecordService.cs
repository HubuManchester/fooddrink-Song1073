using System.Text.Json;

namespace FoodDrinkApp.Services;

/// <summary>
/// Manages daily diet records with JSON-based local persistence.
/// Records are scoped per profile and per day.
/// Switching profiles shows only that profile's records.
/// </summary>
public static class DietRecordService
{
    private static readonly string _filePath = Path.Combine(FileSystem.AppDataDirectory, "diet_records.json");
    private static List<DietRecord> _records = new();

    static DietRecordService()
    {
        LoadRecords();
    }

    // ─── Profile-Scoped Helpers ───

    /// <summary>
    /// Returns the active profile's ID, or empty string if none.
    /// </summary>
    private static string ActiveProfileId =>
        ProfileService.GetActiveProfile()?.Id ?? string.Empty;

    /// <summary>
    /// Returns all records for a specific date scoped to the active profile.
    /// Legacy records (empty ProfileId) are shown to all profiles for backward compat.
    /// </summary>
    public static List<DietRecord> GetRecordsForDate(DateTime? date = null)
    {
        var targetDate = (date ?? DateTime.Today).Date;
        var profileId = ActiveProfileId;
        return _records.Where(r =>
                r.Date.Date == targetDate &&
                (r.ProfileId == profileId || string.IsNullOrEmpty(r.ProfileId)))
                       .OrderBy(r => r.ConsumedAt)
                       .ToList();
    }

    /// <summary>
    /// Returns today's records for the active profile.
    /// </summary>
    public static List<DietRecord> GetTodayRecords() => GetRecordsForDate(DateTime.Today);

    /// <summary>
    /// Adds a diet record and persists.
    /// </summary>
    public static void AddRecord(DietRecord record)
    {
        _records.Add(record);
        Save();
    }

    /// <summary>
    /// Removes a record by ID.
    /// </summary>
    public static void DeleteRecord(string recordId)
    {
        _records.RemoveAll(r => r.Id == recordId);
        Save();
    }

    /// <summary>
    /// Returns the total calories consumed today for the active profile.
    /// </summary>
    public static double GetTodayTotalCalories()
    {
        return GetTodayRecords().Sum(r => r.TotalCalories);
    }

    /// <summary>
    /// Returns the remaining calories for today based on the active profile's target.
    /// </summary>
    public static double GetTodayRemainingCalories()
    {
        var profile = ProfileService.GetActiveProfile();
        if (profile == null) return 0;

        ProfileService.RefreshDailyCalories(profile);
        return profile.DailyCalorieTarget - GetTodayTotalCalories();
    }

    /// <summary>
    /// Returns today's records for the active profile, grouped by meal type.
    /// </summary>
    public static Dictionary<string, List<DietRecord>> GetTodayGroupedByMeal()
    {
        var mealOrder = new[] { "Breakfast", "Lunch", "Dinner", "Snack" };
        var records = GetTodayRecords();
        var result = new Dictionary<string, List<DietRecord>>();

        foreach (var meal in mealOrder)
        {
            var items = records.Where(r => r.MealType.Equals(meal, StringComparison.OrdinalIgnoreCase)).ToList();
            if (items.Count > 0)
                result[meal] = items;
        }

        return result;
    }

    /// <summary>
    /// Calculates today's macro totals for the active profile.
    /// </summary>
    public static (double Protein, double Carbs, double Fat) GetTodayMacros()
    {
        var records = GetTodayRecords();
        return (
            records.Sum(r => r.TotalProtein),
            records.Sum(r => r.TotalCarbs),
            records.Sum(r => r.TotalFat)
        );
    }

    // ─── Persistence ───

    private static void LoadRecords()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                string json = File.ReadAllText(_filePath);
                _records = JsonSerializer.Deserialize<List<DietRecord>>(json) ?? new();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DietRecordService load error: {ex.Message}");
            _records = new();
        }
    }

    private static void Save()
    {
        try
        {
            var json = JsonSerializer.Serialize(_records, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DietRecordService save error: {ex.Message}");
        }
    }
}
