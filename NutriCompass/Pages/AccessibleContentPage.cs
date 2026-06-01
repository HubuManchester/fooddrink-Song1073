using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using NutriCompass.Services;

namespace NutriCompass.Pages;

public class AccessibleContentPage : ContentPage
{
    private AccessibilityService? _accessibilityService;

    protected AccessibilityService AccessibilityService => _accessibilityService ??= Application.Current?.Services
        .GetRequiredService<AccessibilityService>()
        ?? throw new InvalidOperationException("AccessibilityService is not available.");

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
    }
}
