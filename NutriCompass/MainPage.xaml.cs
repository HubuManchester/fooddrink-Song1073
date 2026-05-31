using Microsoft.Extensions.DependencyInjection;
using NutriCompass.ViewModels;

namespace NutriCompass;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        BindingContext = MauiProgram.Services.GetRequiredService<MainPageViewModel>();
    }
}
