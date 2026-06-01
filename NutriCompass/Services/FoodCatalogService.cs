using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using NutriCompass.Models;

namespace NutriCompass.Services;

public sealed class FoodCatalogService
{
    private readonly MockApiConfig _config;
    private readonly HttpClient _httpClient;

    public FoodCatalogService(MockApiConfig config)
    {
        _config = config;
        _httpClient = new HttpClient();
    }

    public async Task<IEnumerable<FoodItem>> GetFoodsAsync()
    {
        if (string.IsNullOrWhiteSpace(_config.EndpointUrl))
        {
            return GetLocalMockData();
        }

        try
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<FoodItem>>(_config.EndpointUrl);
            if (response is not null)
            {
                return response;
            }
        }
        catch
        {
            // swallow and fallback
        }

        return GetLocalMockData();
    }

    private static IEnumerable<FoodItem> GetLocalMockData()
    {
        return new[]
        {
            new FoodItem
            {
                Name = "Berry Kefir Smoothie",
                Category = "Snack",
                Description = "Probiotic-rich kefir blended with berries.",
                MacroSummary = "195 kcal · 8g protein · 4g fat",
                Tags = "smoothie, berry"
            },
            new FoodItem
            {
                Name = "Thai Basil Power Bowl",
                Category = "Main",
                Description = "Jasmine rice, basil-chicken, veggies.",
                MacroSummary = "420 kcal · 32g protein · 12g fat",
                Tags = "thai, bowl"
            },
            new FoodItem
            {
                Name = "Sunrise Avocado Toast",
                Category = "Breakfast",
                Description = "Multigrain toast topped with smashed avocado and seeds.",
                MacroSummary = "305 kcal · 12g protein · 15g fat",
                Tags = "avocado, breakfast"
            }
        };
    }
}
