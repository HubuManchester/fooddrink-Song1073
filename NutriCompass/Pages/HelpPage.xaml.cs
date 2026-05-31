using Microsoft.Extensions.DependencyInjection;
using NutriCompass.ViewModels;

namespace NutriCompass.Pages;

public partial class HelpPage : ContentPage
{
    public HelpPage()
    {
        InitializeComponent();
        BindingContext = MauiProgram.Services.GetRequiredService<HelpPageViewModel>();
    }
}
