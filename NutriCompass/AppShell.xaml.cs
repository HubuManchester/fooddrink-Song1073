using NutriCompass.Pages;

namespace NutriCompass;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute(nameof(AddItemPage), typeof(AddItemPage));
		Routing.RegisterRoute(nameof(FoodDetailPage), typeof(FoodDetailPage));
		Routing.RegisterRoute(nameof(HardwarePage), typeof(HardwarePage));
		Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
	}
}
