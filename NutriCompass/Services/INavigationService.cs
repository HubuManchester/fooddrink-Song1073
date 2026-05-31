namespace NutriCompass.Services;

public interface INavigationService
{
    Task NavigateToHelpAsync();
    Task NavigateBackAsync();
}
