using System.Net.Http.Json;
using System.Text.Json;

namespace FoodDrinkApp.Services;

/// <summary>
/// Manages user profiles with JSON-based local persistence.
/// Supports multiple profiles with a single active profile at a time.
/// Includes API sync for importing profiles from MockAPI.
/// </summary>
public static class ProfileService
{
    private static readonly string _filePath = Path.Combine(FileSystem.AppDataDirectory, "user_profiles.json");
    private static readonly HttpClient _httpClient = new();
    private static List<UserProfile> _profiles = new();

    static ProfileService()
    {
        LoadProfiles();
    }

    // ─── CRUD ───

    public static List<UserProfile> GetAllProfiles() => _profiles.ToList();

    public static UserProfile? GetActiveProfile() => _profiles.FirstOrDefault(p => p.IsActive);

    public static void AddProfile(UserProfile profile)
    {
        // If this is the first profile, auto-activate it
        if (_profiles.Count == 0)
            profile.IsActive = true;

        _profiles.Add(profile);
        Save();
    }

    public static void UpdateProfile(UserProfile profile)
    {
        var index = _profiles.FindIndex(p => p.Id == profile.Id);
        if (index >= 0)
        {
            _profiles[index] = profile;
            Save();
        }
    }

    public static void DeleteProfile(string profileId)
    {
        _profiles.RemoveAll(p => p.Id == profileId);

        // If we deleted the active one, activate the first remaining
        if (!_profiles.Any(p => p.IsActive) && _profiles.Count > 0)
            _profiles[0].IsActive = true;

        Save();
    }

    /// <summary>
    /// Switch the active profile to the one with the given ID.
    /// </summary>
    public static void SetActiveProfile(string profileId)
    {
        foreach (var p in _profiles)
            p.IsActive = (p.Id == profileId);
        Save();
    }

    /// <summary>
    /// Recalculates the daily calorie target for a profile if the date has changed.
    /// Returns true if the value was refreshed.
    /// </summary>
    public static bool RefreshDailyCalories(UserProfile profile)
    {
        if (profile.LastCalculatedDate.Date == DateTime.Today && profile.DailyCalorieTarget > 0)
            return false; // already up-to-date today

        profile.DailyCalorieTarget = profile.CalculateDailyCalories();
        profile.LastCalculatedDate = DateTime.Today;
        UpdateProfile(profile);
        return true;
    }

    // ─── API Sync ───

    /// <summary>
    /// Fetches profiles from MockAPI and merges them into local data.
    /// Only adds new profiles (matched by Name, case-insensitive).
    /// Imported profiles are set to inactive by default.
    /// </summary>
    public static async Task SyncFromApiAsync()
    {
        try
        {
            var apiProfiles = await _httpClient.GetFromJsonAsync<List<UserProfile>>(
                MockApiConfig.ProfilesUrl);

            if (apiProfiles == null || apiProfiles.Count == 0) return;

            bool updated = false;
            foreach (var apiProfile in apiProfiles)
            {
                // Skip if a profile with the same name already exists locally
                var existing = _profiles.FirstOrDefault(p =>
                    p.Name.Equals(apiProfile.Name, StringComparison.OrdinalIgnoreCase));

                if (existing == null)
                {
                    // Imported profiles should not override the current active one
                    apiProfile.IsActive = false;
                    // Assign a fresh local ID to avoid conflicts
                    apiProfile.Id = Guid.NewGuid().ToString();
                    // Calculate daily calories immediately
                    apiProfile.DailyCalorieTarget = apiProfile.CalculateDailyCalories();
                    apiProfile.LastCalculatedDate = DateTime.Today;

                    _profiles.Add(apiProfile);
                    updated = true;
                    System.Diagnostics.Debug.WriteLine($"Profile synced from API: {apiProfile.Name}");
                }
            }

            if (updated)
            {
                // If no profile was active before, activate the first one
                if (!_profiles.Any(p => p.IsActive) && _profiles.Count > 0)
                    _profiles[0].IsActive = true;

                Save();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Profile API sync error: {ex.Message}");
        }
    }

    // ─── Persistence ───

    private static void LoadProfiles()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                string json = File.ReadAllText(_filePath);
                _profiles = JsonSerializer.Deserialize<List<UserProfile>>(json) ?? new();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ProfileService load error: {ex.Message}");
            _profiles = new();
        }
    }

    private static void Save()
    {
        try
        {
            var json = JsonSerializer.Serialize(_profiles, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ProfileService save error: {ex.Message}");
        }
    }
}
