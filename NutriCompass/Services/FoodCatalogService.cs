using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using NutriCompass.Models;

namespace NutriCompass.Services;

public sealed class FoodCatalogService
{
    private readonly MockApiConfig _config;
    private readonly HttpClient _httpClient;
    private readonly List<FoodItem> _fallbackCache = GetLocalMockData().ToList();

    public FoodCatalogService(MockApiConfig config)
    {
        _config = config;
        _httpClient = new HttpClient();
    }

    public Task<FoodCatalogResult> GetFoodsAsync(CancellationToken cancellationToken = default)
    {
        return FetchFoodsAsync(cancellationToken);
    }

    public async Task<FoodCatalogSubmissionResult> SubmitFoodAsync(FoodItem newItem, CancellationToken cancellationToken = default)
    {
        var sanitized = new FoodItem
        {
            Name = newItem.Name.Trim(),
            Category = newItem.Category.Trim(),
            Description = newItem.Description.Trim(),
            MacroSummary = newItem.MacroSummary.Trim(),
            Tags = newItem.Tags?.Trim() ?? string.Empty
        };

        if (string.IsNullOrWhiteSpace(_config.EndpointUrl))
        {
            AddToFallback(sanitized);
            return new FoodCatalogSubmissionResult(true, "Cached locally because endpoint is not configured.");
        }

        try
        {
            var response = await _httpClient.PostAsJsonAsync(_config.EndpointUrl, sanitized, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return new FoodCatalogSubmissionResult(true, "Stored remotely.");
            }
        }
        catch
        {
            // swallow and fallback
        }

        AddToFallback(sanitized);
        return new FoodCatalogSubmissionResult(false, "Remote service unavailable; saved locally.");
    }

    private async Task<FoodCatalogResult> FetchFoodsAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_config.EndpointUrl))
        {
            return new FoodCatalogResult(_fallbackCache, true);
        }

        try
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<FoodItem>>(_config.EndpointUrl, cancellationToken);
            if (response is not null && response.Any())
            {
                return new FoodCatalogResult(response, false);
            }
        }
        catch
        {
            // swallow and fallback
        }

        return new FoodCatalogResult(_fallbackCache, true);
    }

    private void AddToFallback(FoodItem item)
    {
        _fallbackCache.Add(item);
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

public sealed record FoodCatalogResult(IEnumerable<FoodItem> Items, bool UsedFallback);

public sealed record FoodCatalogSubmissionResult(bool IsSuccess, string Message);
