using System;

namespace NutriCompass.Services;

public sealed class HealthCalculatorService : IHealthCalculatorService
{
    public double CalculateBmi(double weightKg, double heightCm)
    {
        if (weightKg <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(weightKg), "Weight must be greater than zero.");
        }

        if (heightCm <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(heightCm), "Height must be greater than zero.");
        }

        var heightMeters = heightCm / 100.0;
        var bmi = weightKg / (heightMeters * heightMeters);
        return Math.Round(bmi, 2, MidpointRounding.AwayFromZero);
    }

    public double CalculateMifflinStJeorDailyCalories(double weightKg, double heightCm, int ageYears, Gender gender)
    {
        if (weightKg <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(weightKg), "Weight must be greater than zero.");
        }

        if (heightCm <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(heightCm), "Height must be greater than zero.");
        }

        if (ageYears <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ageYears), "Age must be greater than zero.");
        }

        var baseMetabolicRate = 10 * weightKg + 6.25 * heightCm - 5 * ageYears;
        var adjustment = gender == Gender.Male ? 5 : -161;
        var calories = baseMetabolicRate + adjustment;
        return Math.Round(calories, 0, MidpointRounding.AwayFromZero);
    }
}
