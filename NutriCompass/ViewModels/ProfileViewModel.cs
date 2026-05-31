using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

using Microsoft.Maui.Controls;
using NutriCompass.Services;

namespace NutriCompass.ViewModels;

public class ProfileViewModel : INotifyPropertyChanged
{
    private readonly IHealthCalculatorService _healthCalculatorService;
    private readonly INavigationService _navigationService;
    private string _heightText = string.Empty;
    private string _weightText = string.Empty;
    private string _ageText = string.Empty;
    private string _bmiResult = string.Empty;
    private string _calorieResult = string.Empty;
    private string _validationMessage = string.Empty;
    private Gender _selectedGender = Gender.Male;

    public ProfileViewModel(IHealthCalculatorService healthCalculatorService, INavigationService navigationService)
    {
        _healthCalculatorService = healthCalculatorService;
        _navigationService = navigationService;
        CalculateCommand = new Command(CalculateAndReport);
        NavigateBackCommand = new Command(async () => await _navigationService.NavigateBackAsync());
    }

    public string HeightText
    {
        get => _heightText;
        set
        {
            if (_heightText == value)
            {
                return;
            }

            _heightText = value;
            OnPropertyChanged();
        }
    }

    public string WeightText
    {
        get => _weightText;
        set
        {
            if (_weightText == value)
            {
                return;
            }

            _weightText = value;
            OnPropertyChanged();
        }
    }

    public string AgeText
    {
        get => _ageText;
        set
        {
            if (_ageText == value)
            {
                return;
            }

            _ageText = value;
            OnPropertyChanged();
        }
    }

    public string BmiResult
    {
        get => _bmiResult;
        set
        {
            if (_bmiResult == value)
            {
                return;
            }

            _bmiResult = value;
            OnPropertyChanged();
        }
    }

    public string CalorieResult
    {
        get => _calorieResult;
        set
        {
            if (_calorieResult == value)
            {
                return;
            }

            _calorieResult = value;
            OnPropertyChanged();
        }
    }

    public string ValidationMessage
    {
        get => _validationMessage;
        set
        {
            if (_validationMessage == value)
            {
                return;
            }

            _validationMessage = value;
            OnPropertyChanged();
        }
    }

    public ICommand CalculateCommand { get; }
    public ICommand NavigateBackCommand { get; }

    public Gender SelectedGender
    {
        get => _selectedGender;
        set
        {
            if (_selectedGender == value)
            {
                return;
            }

            _selectedGender = value;
            OnPropertyChanged();
        }
    }

    public IReadOnlyList<Gender> GenderOptions { get; } = Enum.GetValues<Gender>();

    public event PropertyChangedEventHandler? PropertyChanged;

    private void CalculateAndReport()
    {
        ValidationMessage = string.Empty;
        BmiResult = string.Empty;
        CalorieResult = string.Empty;

        if (!double.TryParse(WeightText, out var weight) || weight <= 0)
        {
            ValidationMessage = "Weight must be a positive number.";
            return;
        }

        if (!double.TryParse(HeightText, out var height) || height <= 0)
        {
            ValidationMessage = "Height must be a positive number.";
            return;
        }

        if (!int.TryParse(AgeText, out var age) || age <= 0)
        {
            ValidationMessage = "Age must be a positive whole number.";
            return;
        }

        try
        {
            var bmi = _healthCalculatorService.CalculateBmi(weight, height);
            var calories = _healthCalculatorService.CalculateMifflinStJeorDailyCalories(weight, height, age, SelectedGender);
            BmiResult = $"BMI: {bmi:F2}";
            CalorieResult = $"Calories: {calories:N0} kcal";
        }
        catch (ArgumentOutOfRangeException ex)
        {
            ValidationMessage = ex.Message;
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
