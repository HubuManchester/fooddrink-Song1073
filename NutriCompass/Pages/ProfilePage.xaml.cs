using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using NutriCompass.ViewModels;

namespace NutriCompass.Pages;

public partial class ProfilePage : ContentPage
{
    public ProfilePage()
    {
        InitializeComponent();
        BindingContext = MauiProgram.Services.GetRequiredService<ProfileViewModel>();
    }
}
