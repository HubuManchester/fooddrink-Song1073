using System.Net.Http.Json;
using System.Text.Json;
using FoodDrinkApp.Models;

namespace FoodDrinkApp.Services;

public static class FoodCatalogService
{
    private static readonly HttpClient _httpClient = new HttpClient();
    private static readonly string _localFilePath = Path.Combine(FileSystem.AppDataDirectory, "foods_data.json");
    private static List<FoodItem> _localData = new List<FoodItem>();

    /// <summary>
    /// The six predefined valid categories, in display order.
    /// </summary>
    public static readonly List<string> ValidCategories = new()
    {
        "Breakfast", "Lunch", "Dinner", "Drink", "Snack", "Dessert"
    };

    /// <summary>
    /// Returns the predefined display order index for a category (lower = first).
    /// Unknown categories are placed at the end.
    /// </summary>
    public static int CategoryOrder(string category)
    {
        var index = ValidCategories.FindIndex(c => c.Equals(category, StringComparison.OrdinalIgnoreCase));
        return index >= 0 ? index : int.MaxValue;
    }

    /// <summary>
    /// Checks whether a category string matches one of the six valid categories.
    /// </summary>
    private static bool IsValidCategory(string? category) =>
        !string.IsNullOrWhiteSpace(category) &&
        ValidCategories.Any(c => c.Equals(category, StringComparison.OrdinalIgnoreCase));

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

            // If the persisted data has no items with valid categories (e.g. corrupted by
            // a previous API sync with garbage data), reset to the built-in defaults.
            if (!_localData.Any(f => IsValidCategory(f.Category)))
            {
                System.Diagnostics.Debug.WriteLine("Local data has no valid categories – resetting to defaults.");
                _localData = GetDefaultFoodItems();
                SaveLocalData();
            }
        }
        else
        {
            _localData = GetDefaultFoodItems();
            SaveLocalData();
        }
    }

    /// <summary>
    /// Returns a rich set of default food items across all categories for a polished first-run experience.
    /// </summary>
    private static List<FoodItem> GetDefaultFoodItems()
    {
        return new List<FoodItem>
        {
            // Breakfast
            new FoodItem { Id = "1", Name = "Berry Yogurt Bowl", Category = "Breakfast", Description = "Creamy Greek yogurt layered with fresh mixed berries, honey drizzle, and crunchy granola clusters.", Calories = 340, Protein = 24, Carbs = 42, Fat = 8, AllergyNote = "Dairy, Nuts", Tags = "Healthy, Quick" },
            new FoodItem { Id = "2", Name = "Avocado Toast", Category = "Breakfast", Description = "Sourdough toast topped with smashed avocado, cherry tomatoes, microgreens, and a poached egg.", Calories = 380, Protein = 14, Carbs = 32, Fat = 22, AllergyNote = "Gluten, Eggs", Tags = "Healthy, Trendy" },
            new FoodItem { Id = "3", Name = "Banana Oatmeal", Category = "Breakfast", Description = "Warm steel-cut oats with caramelised banana slices, cinnamon, and a drizzle of maple syrup.", Calories = 310, Protein = 10, Carbs = 58, Fat = 6, AllergyNote = "Gluten", Tags = "Warm, Comforting" },
            new FoodItem { Id = "4", Name = "Eggs Benedict", Category = "Breakfast", Description = "English muffin with Canadian bacon, perfectly poached eggs, and silky hollandaise sauce.", Calories = 480, Protein = 26, Carbs = 28, Fat = 30, AllergyNote = "Gluten, Eggs, Dairy", Tags = "Classic, Brunch" },

            // Lunch
            new FoodItem { Id = "5", Name = "Chicken Breast Box", Category = "Lunch", Description = "Herb-grilled chicken breast served with brown rice, steamed broccoli, and a side of hummus.", Calories = 520, Protein = 38, Carbs = 58, Fat = 14, AllergyNote = "None", Tags = "Protein, Meal Prep" },
            new FoodItem { Id = "6", Name = "Hummus with Pita", Category = "Lunch", Description = "Smooth chickpea hummus served with warm pita bread, cherry tomatoes, and cucumber slices.", Calories = 290, Protein = 12, Carbs = 38, Fat = 10, AllergyNote = "Gluten, Sesame", Tags = "Vegetarian, Quick" },
            new FoodItem { Id = "7", Name = "Caesar Salad", Category = "Lunch", Description = "Crisp romaine lettuce with garlic croutons, shaved parmesan, and creamy Caesar dressing.", Calories = 350, Protein = 15, Carbs = 22, Fat = 24, AllergyNote = "Dairy, Gluten, Eggs", Tags = "Classic, Light" },

            // Dinner
            new FoodItem { Id = "8", Name = "Grilled Salmon", Category = "Dinner", Description = "Atlantic salmon fillet grilled to perfection with lemon-dill sauce and roasted vegetables.", Calories = 450, Protein = 42, Carbs = 12, Fat = 28, AllergyNote = "Fish", Tags = "Omega-3, Healthy" },
            new FoodItem { Id = "9", Name = "Beef Stir-Fry", Category = "Dinner", Description = "Tender beef strips wok-fried with colourful bell peppers, snap peas, and a savory soy-ginger glaze.", Calories = 560, Protein = 35, Carbs = 45, Fat = 22, AllergyNote = "Soy, Gluten", Tags = "Asian, Protein" },
            new FoodItem { Id = "10", Name = "Pasta Primavera", Category = "Dinner", Description = "Al dente penne tossed with seasonal vegetables in a light garlic-herb olive oil sauce.", Calories = 420, Protein = 16, Carbs = 62, Fat = 14, AllergyNote = "Gluten", Tags = "Vegetarian, Italian" },

            // Drink
            new FoodItem { Id = "11", Name = "Fresh Lemonade", Category = "Drink", Description = "Freshly squeezed lemons blended with sparkling water, mint leaves, and a touch of honey.", Calories = 80, Protein = 0, Carbs = 22, Fat = 0, AllergyNote = "None", Tags = "Refreshing, Summer" },
            new FoodItem { Id = "12", Name = "Mango Lassi", Category = "Drink", Description = "Luscious mango blended with creamy yogurt, cardamom, and a hint of saffron.", Calories = 180, Protein = 6, Carbs = 32, Fat = 4, AllergyNote = "Dairy", Tags = "Indian, Creamy" },
            new FoodItem { Id = "13", Name = "Green Smoothie", Category = "Drink", Description = "Spinach, banana, almond milk, and chia seeds blended into a nutrient-packed smoothie.", Calories = 150, Protein = 5, Carbs = 28, Fat = 4, AllergyNote = "Nuts", Tags = "Healthy, Detox" },

            // Snack
            new FoodItem { Id = "14", Name = "Fruit Smoothie Bowl", Category = "Snack", Description = "Thick açaí smoothie base topped with sliced fruits, coconut flakes, and crunchy granola.", Calories = 280, Protein = 8, Carbs = 48, Fat = 8, AllergyNote = "Nuts, Dairy", Tags = "Superfood, Colourful" },
            new FoodItem { Id = "15", Name = "Trail Mix", Category = "Snack", Description = "A satisfying blend of almonds, cashews, dark chocolate chips, dried cranberries, and pumpkin seeds.", Calories = 220, Protein = 7, Carbs = 24, Fat = 12, AllergyNote = "Nuts", Tags = "Energy, Portable" },

            // Dessert
            new FoodItem { Id = "16", Name = "Chocolate Lava Cake", Category = "Dessert", Description = "Decadent individual chocolate cake with a warm, gooey molten centre. Served with vanilla ice cream.", Calories = 480, Protein = 8, Carbs = 52, Fat = 28, AllergyNote = "Dairy, Gluten, Eggs", Tags = "Indulgent, Warm" },
            new FoodItem { Id = "17", Name = "Tiramisu", Category = "Dessert", Description = "Classic Italian dessert with layers of espresso-soaked ladyfingers and mascarpone cream.", Calories = 350, Protein = 6, Carbs = 38, Fat = 20, AllergyNote = "Dairy, Gluten, Eggs", Tags = "Italian, Coffee" },
        };
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
                // Only accept API items whose Category is one of the six valid values.
                // This prevents garbage data (e.g. "category 1") from overwriting local defaults.
                var validItems = response.Where(f => IsValidCategory(f.Category)).ToList();

                if (validItems.Count > 0)
                {
                    // Merge strategy: upsert API items into local data instead of replacing.
                    // This preserves default/local-only items that are not in the API response.
                    foreach (var apiItem in validItems)
                    {
                        var existingIndex = _localData.FindIndex(f =>
                            f.Id == apiItem.Id ||
                            f.Name.Equals(apiItem.Name, StringComparison.OrdinalIgnoreCase));

                        if (existingIndex >= 0)
                        {
                            // Update existing item with latest API data
                            _localData[existingIndex] = apiItem;
                        }
                        else
                        {
                            // New item from API — append it
                            _localData.Add(apiItem);
                        }
                    }
                    SaveLocalData(); // 将合并后的数据备份到本地
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("API returned no items with valid categories – keeping local data.");
                }
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

    /// <summary>
    /// Returns all distinct categories from the current data.
    /// </summary>
    public static async Task<List<string>> GetCategoriesAsync()
    {
        var foods = await GetFoodsAsync();
        return foods.Select(f => f.Category).Distinct().OrderBy(c => c).ToList();
    }

    /// <summary>
    /// Returns foods grouped by their category for section-based display.
    /// </summary>
    public static async Task<Dictionary<string, List<FoodItem>>> GetFoodsGroupedByCategoryAsync()
    {
        var foods = await GetFoodsAsync();
        return foods.GroupBy(f => f.Category)
                    .ToDictionary(g => g.Key, g => g.ToList());
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