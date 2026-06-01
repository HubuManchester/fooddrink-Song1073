using System.Net.Http.Json;
using System.Text.Json;
using FoodDrinkApp.Models;

namespace FoodDrinkApp.Services;

public static class FoodCatalogService
{
    private static readonly HttpClient _httpClient = new HttpClient();
    private static readonly string _localFilePath = Path.Combine(FileSystem.AppDataDirectory, "foods_data.json");
    private static List<FoodItem> _localData = new List<FoodItem>();

    static FoodCatalogService()
    {
        LoadLocalData();
    }

    private static void LoadLocalData()
    {
        if (File.Exists(_localFilePath))
        {
            string json = File.ReadAllText(_localFilePath);
            _localData = JsonSerializer.Deserialize<List<FoodItem>>(json) ?? new List<FoodItem>();
        }
        else
        {
            _localData = new List<FoodItem>
            {
                new FoodItem { Id = "1", Name = "Berry Yogurt Bowl", Category = "Breakfast", Description = "Greek yogurt paired with mixed berries.", Calories = 340, Protein = 24, Carbs = 42, Fat = 8, AllergyNote = "Dairy", Tags = "Healthy Breakfast" },
                new FoodItem { Id = "2", Name = "Chicken Breast Box", Category = "Lunch", Description = "Grilled chicken breast, brown rice.", Calories = 520, Protein = 38, Carbs = 58, Fat = 14, AllergyNote = "None", Tags = "Protein Lunch" }
            };
            SaveLocalData();
        }
    }

    private static void SaveLocalData()
    {
        string json = JsonSerializer.Serialize(_localData);
        File.WriteAllText(_localFilePath, json);
    }

    public static async Task<List<FoodItem>> GetFoodsAsync(string query = "")
    {
        List<FoodItem> currentData = _localData;

        if (string.IsNullOrWhiteSpace(query))
            return currentData;

        query = query.ToLower();
        return currentData.Where(f =>
            (f.Name != null && f.Name.ToLower().Contains(query)) ||
            (f.Category != null && f.Category.ToLower().Contains(query)) ||
            (f.Tags != null && f.Tags.ToLower().Contains(query))
        ).ToList();
    }

    public static async Task AddFoodAsync(FoodItem item)
    {
        if (string.IsNullOrEmpty(item.Id))
        {
            item.Id = Guid.NewGuid().ToString();
        }

        _localData.Add(item);
        SaveLocalData();
    }

    // 👇 新增的刪除功能 👇
    public static async Task DeleteFoodAsync(string id)
    {
        var item = _localData.FirstOrDefault(f => f.Id == id);
        if (item != null)
        {
            _localData.Remove(item);
            SaveLocalData(); // 同步更新到本地檔案
        }
    }
}