using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

using Microsoft.Maui.Controls;
using NutriCompass.Services;

namespace NutriCompass.ViewModels;

public class HelpPageViewModel : INotifyPropertyChanged
{
    private readonly INavigationService _navigationService;
    private string _topicFilter = string.Empty;

    public HelpPageViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
        GoHomeCommand = new Command(async () => await _navigationService.NavigateBackAsync());
    }

    public string TopicFilter
    {
        get => _topicFilter;
        set
        {
            if (_topicFilter == value)
            {
                return;
            }

            _topicFilter = value;
            OnPropertyChanged();
        }
    }

    public string Intro => "Use this section to ramp up quickly with accessibility, hardware linkages, and test plans.";

    public ICommand GoHomeCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
