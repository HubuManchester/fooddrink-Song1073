using FoodDrinkApp.Services;

namespace FoodDrinkApp.Views;

public partial class SettingsPage : ContentPage
{
    private bool _isInitializing = true;

    public SettingsPage()
    {
        InitializeComponent();

        // Use RequestedTheme (the *actual* effective theme) instead of UserAppTheme
        // (which is Unspecified when following the system). This ensures the switch
        // accurately reflects the current visual state on first load.
        ThemeSwitch.IsToggled = Application.Current?.RequestedTheme == AppTheme.Dark;
        LargeTextSwitch.IsToggled = AccessibilityService.LargeTextEnabled;

        _isInitializing = false;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);

        // Re-sync the switch state every time the page appears,
        // in case the system theme changed while user was on another tab.
        _isInitializing = true;
        ThemeSwitch.IsToggled = Application.Current?.RequestedTheme == AppTheme.Dark;
        _isInitializing = false;
    }

    private void OnThemeToggled(object? sender, ToggledEventArgs e)
    {
        // Skip if we're just syncing the initial state (prevents double-trigger)
        if (_isInitializing) return;

        if (Application.Current != null)
        {
            Application.Current.UserAppTheme = e.Value ? AppTheme.Dark : AppTheme.Light;
        }
    }

    private void OnLargeTextToggled(object? sender, ToggledEventArgs e)
    {
        if (_isInitializing) return;

        AccessibilityService.LargeTextEnabled = e.Value;
        AccessibilityService.ApplyFontScale(this);
    }
}