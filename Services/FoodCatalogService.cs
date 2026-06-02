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
    /// The eight predefined valid food type categories, in display order.
    /// NOTE: Meal time (Breakfast/Lunch/Dinner/Snack) is on DietRecord, NOT here.
    /// </summary>
    public static readonly List<string> ValidCategories = new()
    {
        "Grain", "Meat", "Vegetable", "Fruit", "Dairy", "Seafood", "Beverage", "Dessert"
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
            else
            {
                // Back-fill missing image URLs from defaults (upgrade path).
                // When we add images to default data, existing local items may lack them.
                var defaults = GetDefaultFoodItems();
                bool updated = false;
                foreach (var def in defaults)
                {
                    if (string.IsNullOrEmpty(def.ImagePath)) continue;
                    var local = _localData.FirstOrDefault(f =>
                        f.Id == def.Id || f.Name.Equals(def.Name, StringComparison.OrdinalIgnoreCase));
                    if (local != null && string.IsNullOrEmpty(local.ImagePath))
                    {
                        local.ImagePath = def.ImagePath;
                        updated = true;
                    }
                }
                if (updated) SaveLocalData();
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
            // Grain — breads, rice, cereals, pasta
            new FoodItem { Id = "1", Name = "Avocado Toast", Category = "Grain", Description = "Sourdough toast topped with smashed avocado, cherry tomatoes, microgreens, and a poached egg.", Calories = 380, Protein = 14, Carbs = 32, Fat = 22, AllergyNote = "Gluten, Eggs", Tags = "Healthy, Trendy",
                ImagePath = "https://images.unsplash.com/photo-1541519227354-08fa5d50c44d?w=400&h=300&fit=crop" },
            new FoodItem { Id = "2", Name = "Banana Oatmeal", Category = "Grain", Description = "Warm steel-cut oats with caramelised banana slices, cinnamon, and a drizzle of maple syrup.", Calories = 310, Protein = 10, Carbs = 58, Fat = 6, AllergyNote = "Gluten", Tags = "Warm, Comforting",
                ImagePath = "https://images.unsplash.com/photo-1517673400267-0251440c45dc?w=400&h=300&fit=crop" },
            new FoodItem { Id = "3", Name = "Pasta Primavera", Category = "Grain", Description = "Al dente penne tossed with seasonal vegetables in a light garlic-herb olive oil sauce.", Calories = 420, Protein = 16, Carbs = 62, Fat = 14, AllergyNote = "Gluten", Tags = "Vegetarian, Italian",
                ImagePath = "https://images.unsplash.com/photo-1621996346565-e3dbc646d9a9?w=400&h=300&fit=crop" },

            // Meat — poultry, beef, pork, eggs
            new FoodItem { Id = "5", Name = "Chicken Breast Box", Category = "Meat", Description = "Herb-grilled chicken breast served with brown rice, steamed broccoli, and a side of hummus.", Calories = 520, Protein = 38, Carbs = 58, Fat = 14, AllergyNote = "None", Tags = "Protein, Meal Prep",
                ImagePath = "https://images.unsplash.com/photo-1532550907401-a500c9a57435?w=400&h=300&fit=crop" },
            new FoodItem { Id = "6", Name = "Beef Stir-Fry", Category = "Meat", Description = "Tender beef strips wok-fried with colourful bell peppers, snap peas, and a savory soy-ginger glaze.", Calories = 560, Protein = 35, Carbs = 45, Fat = 22, AllergyNote = "Soy, Gluten", Tags = "Asian, Protein",
                ImagePath = "https://images.unsplash.com/photo-1603133872878-684f208fb84b?w=400&h=300&fit=crop" },

            // Vegetable — salads, greens
            new FoodItem { Id = "8", Name = "Caesar Salad", Category = "Vegetable", Description = "Crisp romaine lettuce with garlic croutons, shaved parmesan, and creamy Caesar dressing.", Calories = 350, Protein = 15, Carbs = 22, Fat = 24, AllergyNote = "Dairy, Gluten, Eggs", Tags = "Classic, Light",
                ImagePath = "https://images.unsplash.com/photo-1550304943-4f24f54ddde9?w=400&h=300&fit=crop" },

            // Fruit — fresh fruits, fruit bowls
            new FoodItem { Id = "9", Name = "Fruit Smoothie Bowl", Category = "Fruit", Description = "Thick açaí smoothie base topped with sliced fruits, coconut flakes, and crunchy granola.", Calories = 280, Protein = 8, Carbs = 48, Fat = 8, AllergyNote = "Nuts, Dairy", Tags = "Superfood, Colourful",
                ImagePath = "https://images.unsplash.com/photo-1590301157890-4810ed352733?w=400&h=300&fit=crop" },

            // Dairy — yogurt, cheese, milk
            new FoodItem { Id = "11", Name = "Berry Yogurt Bowl", Category = "Dairy", Description = "Creamy Greek yogurt layered with fresh mixed berries, honey drizzle, and crunchy granola clusters.", Calories = 340, Protein = 24, Carbs = 42, Fat = 8, AllergyNote = "Dairy, Nuts", Tags = "Healthy, Quick",
                ImagePath = "https://images.unsplash.com/photo-1488477181946-6428a0291777?w=400&h=300&fit=crop" },

            // Seafood — fish, shellfish
            new FoodItem { Id = "12", Name = "Grilled Salmon", Category = "Seafood", Description = "Atlantic salmon fillet grilled to perfection with lemon-dill sauce and roasted vegetables.", Calories = 450, Protein = 42, Carbs = 12, Fat = 28, AllergyNote = "Fish", Tags = "Omega-3, Healthy",
                ImagePath = "https://images.unsplash.com/photo-1467003909585-2f8a72700288?w=400&h=300&fit=crop" },

            // Beverage — drinks, smoothies
            new FoodItem { Id = "13", Name = "Fresh Lemonade", Category = "Beverage", Description = "Freshly squeezed lemons blended with sparkling water, mint leaves, and a touch of honey.", Calories = 80, Protein = 0, Carbs = 22, Fat = 0, AllergyNote = "None", Tags = "Refreshing, Summer",
                ImagePath = "https://images.unsplash.com/photo-1621263764928-df1444c5e859?w=400&h=300&fit=crop" },
            new FoodItem { Id = "14", Name = "Mango Lassi", Category = "Beverage", Description = "Luscious mango blended with creamy yogurt, cardamom, and a hint of saffron.", Calories = 180, Protein = 6, Carbs = 32, Fat = 4, AllergyNote = "Dairy", Tags = "Indian, Creamy",
                ImagePath = "https://images.unsplash.com/photo-1527661591475-527312dd65f5?w=400&h=300&fit=crop" },
            new FoodItem { Id = "15", Name = "Green Smoothie", Category = "Beverage", Description = "Spinach, banana, almond milk, and chia seeds blended into a nutrient-packed smoothie.", Calories = 150, Protein = 5, Carbs = 28, Fat = 4, AllergyNote = "Nuts", Tags = "Healthy, Detox",
                ImagePath = "https://images.unsplash.com/photo-1610970881699-44a5587cabec?w=400&h=300&fit=crop" },

            // Dessert — sweets, cakes
            new FoodItem { Id = "16", Name = "Chocolate Lava Cake", Category = "Dessert", Description = "Decadent individual chocolate cake with a warm, gooey molten centre. Served with vanilla ice cream.", Calories = 480, Protein = 8, Carbs = 52, Fat = 28, AllergyNote = "Dairy, Gluten, Eggs", Tags = "Indulgent, Warm",
                ImagePath = "https://images.unsplash.com/photo-1624353365286-3f8d62daad51?w=400&h=300&fit=crop" },
            new FoodItem { Id = "17", Name = "Tiramisu", Category = "Dessert", Description = "Classic Italian dessert with layers of espresso-soaked ladyfingers and mascarpone cream.", Calories = 350, Protein = 6, Carbs = 38, Fat = 20, AllergyNote = "Dairy, Gluten, Eggs", Tags = "Italian, Coffee",
                ImagePath = "https://images.unsplash.com/photo-1571877227200-a0d98ea607e9?w=400&h=300&fit=crop" },
        };
    }

    private static void SaveLocalData()
    {
        string json = JsonSerializer.Serialize(_localData);
        File.WriteAllText(_localFilePath, json);
    }

    // ── Query with cloud-first, local fallback ──
    public static async Task<List<FoodItem>> GetFoodsAsync(string query = "")
    {
        try
        {
            // Attempt to fetch the latest data from MockAPI
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
                    SaveLocalData(); // Backup the merged data locally
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("API returned no items with valid categories – keeping local data.");
                }
            }
        }
        catch (Exception ex)
        {
            // Catch exceptions in case of network or API errors; subsequent code will use _localData as an offline fallback
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

    // ── Add with dual-write strategy (cloud + local) ──
    public static async Task AddFoodAsync(FoodItem item)
    {
        // Generate a temporary local ID first
        if (string.IsNullOrEmpty(item.Id))
        {
            item.Id = Guid.NewGuid().ToString();
        }

        try
        {
            // Attempt to send a POST request to MockAPI
            var response = await _httpClient.PostAsJsonAsync(MockApiConfig.EndpointUrl, item);
            if (response.IsSuccessStatusCode)
            {
                // If creation is successful on the cloud, replace the temporary ID with the real ID generated by the cloud
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

        // Regardless of cloud success, display and save locally for immediate UI response
        _localData.Add(item);
        SaveLocalData();
    }

    // ── Delete with dual-delete strategy (cloud + local) ──
    public static async Task DeleteFoodAsync(string id)
    {
        try
        {
            // Attempt to send a DELETE request to MockAPI
            string deleteUrl = $"{MockApiConfig.EndpointUrl}/{id}";
            await _httpClient.DeleteAsync(deleteUrl);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"API Delete Error: {ex.Message}");
        }

        // Regardless of cloud success, remove it from local memory and storage
        var item = _localData.FirstOrDefault(f => f.Id == id);
        if (item != null)
        {
            _localData.Remove(item);
            SaveLocalData();
        }
    }
}