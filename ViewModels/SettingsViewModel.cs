using CommunityToolkit.Mvvm.ComponentModel;
using FoodDrinkApp.Services;

namespace FoodDrinkApp.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    private bool _isInitializing = true;

    [ObservableProperty]
    private bool _isDarkTheme;

    [ObservableProperty]
    private bool _isLargeTextEnabled;

    public void Initialize()
    {
        _isInitializing = true;
        IsDarkTheme = Application.Current?.RequestedTheme == AppTheme.Dark;
        IsLargeTextEnabled = AccessibilityService.LargeTextEnabled;
        _isInitializing = false;
    }

    partial void OnIsDarkThemeChanged(bool value)
    {
        if (_isInitializing) return;

        if (Application.Current != null)
        {
            Application.Current.UserAppTheme = value ? AppTheme.Dark : AppTheme.Light;
        }
    }

    partial void OnIsLargeTextEnabledChanged(bool value)
    {
        if (_isInitializing) return;

        AccessibilityService.LargeTextEnabled = value;
        if (Shell.Current?.CurrentPage != null)
        {
            AccessibilityService.ApplyFontScale(Shell.Current.CurrentPage);
        }
    }
}
