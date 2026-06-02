using FoodDrinkApp.Views;

namespace FoodDrinkApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register navigation routes for application subpages to enable Shell.Current.GoToAsync navigation
        Routing.RegisterRoute("AddItemPage", typeof(AddItemPage));
        Routing.RegisterRoute("FoodDetailPage", typeof(FoodDetailPage));
        Routing.RegisterRoute("EditProfilePage", typeof(EditProfilePage));
    }
}