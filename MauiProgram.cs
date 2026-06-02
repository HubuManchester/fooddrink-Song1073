using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui; 

namespace FoodDrinkApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit() 
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
                fonts.AddFont("FluentSystemIcons-Regular.ttf", "FluentIcons");
                fonts.AddFont("PlayfairDisplay-Bold.ttf", "PlayfairBold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}