using NutriCompass.Pages;

namespace NutriCompass;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute(nameof(HelpPage), typeof(HelpPage));
	}
}
