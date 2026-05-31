using Microsoft.Maui.Controls;

namespace NutriCompass.Services;

public class ShellNavigationService : INavigationService
{
    private readonly Shell _shell;

    public ShellNavigationService(AppShell shell)
    {
        _shell = shell;
    }

    public async Task NavigateToHelpAsync()
    {
        await _shell.GoToAsync("//HelpPage");
    }

    public async Task NavigateBackAsync()
    {
        await _shell.GoToAsync("//MainPage");
    }

    public async Task NavigateToProfileAsync()
    {
        await _shell.GoToAsync("//ProfilePage");
    }
}
