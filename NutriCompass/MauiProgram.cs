using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NutriCompass.Services;
using NutriCompass.ViewModels;
using System;

namespace NutriCompass;

public static class MauiProgram
{
    public static IServiceProvider Services { get; private set; } = default!;

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

        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingleton<INavigationService, ShellNavigationService>();
        builder.Services.AddSingleton<MainPageViewModel>();
        builder.Services.AddSingleton<HelpPageViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();
        Services = app.Services;
        return app;
    }
}
