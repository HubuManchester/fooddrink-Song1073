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

    // 👇 升级后的查询功能：优先云端拉取，失败则用本地缓存兜底 👇
    public static async Task<List<FoodItem>> GetFoodsAsync(string query = "")
    {
        try
        {
            // 尝试从 MockAPI 获取最新数据
            var response = await _httpClient.GetFromJsonAsync<List<FoodItem>>(MockApiConfig.EndpointUrl);
            if (response != null)
            {
                _localData = response;
                SaveLocalData(); // 将云端最新数据备份到本地
            }
        }
        catch (Exception ex)
        {
            // 如果断网或 API 错误，捕获异常，后续代码会自动使用 _localData 作为离线兜底
            System.Diagnostics.Debug.WriteLine($"API Get Error: {ex.Message}");
        }

        List<FoodItem> currentData = _localData;

        if (string.IsNullOrWhiteSpace(query))
            return currentData.ToList();

        query = query.ToLower();
        return currentData.Where(f =>
            (f.Name != null && f.Name.ToLower().Contains(query)) ||
            (f.Category != null && f.Category.ToLower().Contains(query)) ||
            (f.Tags != null && f.Tags.ToLower().Contains(query))
        ).ToList();
    }

    // 👇 升级后的添加功能：双写策略（云端+本地） 👇
    public static async Task AddFoodAsync(FoodItem item)
    {
        // 先生成一个临时的本地 ID
        if (string.IsNullOrEmpty(item.Id))
        {
            item.Id = Guid.NewGuid().ToString();
        }

        try
        {
            // 尝试向 MockAPI 发送 POST 请求
            var response = await _httpClient.PostAsJsonAsync(MockApiConfig.EndpointUrl, item);
            if (response.IsSuccessStatusCode)
            {
                // 如果云端创建成功，把临时 ID 替换为云端生成的真实 ID
                var createdItem = await response.Content.ReadFromJsonAsync<FoodItem>();
                if (createdItem != null)
                {
                    item.Id = createdItem.Id;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"API Post Error: {ex.Message}");
        }

        // 无论云端是否成功，都在本地直接显示并保存（极速响应）
        _localData.Add(item);
        SaveLocalData();
    }

    // 👇 升级后的删除功能：双删策略（云端+本地） 👇
    public static async Task DeleteFoodAsync(string id)
    {
        try
        {
            // 尝试向 MockAPI 发送 DELETE 请求
            string deleteUrl = $"{MockApiConfig.EndpointUrl}/{id}";
            await _httpClient.DeleteAsync(deleteUrl);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"API Delete Error: {ex.Message}");
        }

        // 无论云端是否成功，都在本地内存和文件中移除它
        var item = _localData.FirstOrDefault(f => f.Id == id);
        if (item != null)
        {
            _localData.Remove(item);
            SaveLocalData();
        }
    }
}