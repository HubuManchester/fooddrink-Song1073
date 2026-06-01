using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NutriCompass.Pages;
using NutriCompass.Services;

namespace NutriCompass;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<AccessibilityService>();
        builder.Services.AddSingleton<MockApiConfig>();
        builder.Services.AddSingleton<FoodCatalogService>();
        builder.Services.AddSingleton<FoodPage>();
        builder.Services.AddSingleton<HardwarePage>();
        builder.Services.AddSingleton<SettingsPage>();
        builder.Services.AddTransient<AddItemPage>();
        builder.Services.AddTransient<FoodDetailPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
