namespace NutriCompass.Services;

public enum Gender
{
    Male,
    Female
}

public interface IHealthCalculatorService
{
    double CalculateBmi(double weightKg, double heightCm);
    double CalculateMifflinStJeorDailyCalories(double weightKg, double heightCm, int ageYears, Gender gender);
}
