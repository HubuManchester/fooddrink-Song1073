using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

using Microsoft.Maui.Controls;
using NutriCompass.Services;

namespace NutriCompass.ViewModels;

public class MainPageViewModel : INotifyPropertyChanged
{
    private readonly INavigationService _navigationService;

    public MainPageViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
        Highlights = new ObservableCollection<HighlightSection>
        {
            new HighlightSection(
                "Accessibility Ready",
                "Every control exposes AutomationProperties and meets WCAG guided contrast ratios."),
            new HighlightSection(
                "Responsive Layout",
                "Adaptive spacing ensures touch targets stay large on small and large screens."),
            new HighlightSection(
                "Guided Support",
                "Quick links explain how to extend this shell with your own hardware features.")
        };

        HelpCommand = new Command(async () => await _navigationService.NavigateToHelpAsync());
        ProfileCommand = new Command(async () => await _navigationService.NavigateToProfileAsync());
    }

    public string PageTitle => "NutriCompass Overview";

    public string BannerHeading => "Guided Nutrition Compass";

    public string BannerSubtitle => "Focus on accessibility, clarity, and actionable insights.";

    public string SummaryText => "Leverage WCAG friendly palettes, automation annotations, and semantic stacks to keep every interaction predictable.";

    public string SearchPlaceholder => "Search for a food or habit";

    public ObservableCollection<HighlightSection> Highlights { get; }

    public ICommand HelpCommand { get; }
    public ICommand ProfileCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public sealed record HighlightSection(string Title, string Description);
